using AirwayAPI.Application;
using AirwayAPI.Assets;
using AirwayAPI.Data;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;

namespace AirwayAPI.Services
{
    public class EmailService(ILogger<EmailService> logger, IConfiguration configuration, eHelpDeskContext context) : IEmailService
    {
        private readonly ILogger<EmailService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly eHelpDeskContext _context = context;

        public async Task SendEmailAsync(EmailInputBase emailInput)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail}", emailInput.ToEmails);

            // 1) If we have a username, fetch the sender info for placeholders (Name, JobTitle, Phones, etc.)
            //    We'll only do this if username/password is provided. Otherwise, we skip quietly.
            if (!string.IsNullOrWhiteSpace(emailInput.UserName) &&
                !string.IsNullOrWhiteSpace(emailInput.Password))
            {
                // Get DB data for the user
                var (fullName, userEmail, jobTitle, directPhone, mobilePhone) = await GetSenderInfoAsync(emailInput.UserName);

                // Ensure placeholders is not null
                if (emailInput.Placeholders == null)
                    emailInput.Placeholders = new Dictionary<string, string>();

                // Fill placeholders if the caller didn't already set them
                if (!emailInput.Placeholders.ContainsKey("%%NAME%%"))
                    emailInput.Placeholders["%%NAME%%"] = fullName;
                if (!emailInput.Placeholders.ContainsKey("%%JOBTITLE%%"))
                    emailInput.Placeholders["%%JOBTITLE%%"] = jobTitle;
                if (!emailInput.Placeholders.ContainsKey("%%DIRECT%%"))
                    emailInput.Placeholders["%%DIRECT%%"] = directPhone;
                if (!emailInput.Placeholders.ContainsKey("%%MOBILE%%"))
                    emailInput.Placeholders["%%MOBILE%%"] = mobilePhone;

                // If Body is present, map it to %%EMAILBODY%% so it shows up in the template
                if (!emailInput.Placeholders.ContainsKey("%%EMAILBODY%%"))
                    emailInput.Placeholders["%%EMAILBODY%%"] = emailInput.Body ?? string.Empty;

                // If FromEmail is empty, default to the user’s "username@airway.com"
                if (string.IsNullOrWhiteSpace(emailInput.FromEmail))
                {
                    emailInput.FromEmail = $"{emailInput.UserName}@airway.com";
                }
            }
            else
            {
                // If the user didn't provide login info, we skip the step of auto-filling placeholders
                // (You could optionally throw if these are required.)
                _logger.LogInformation("No username/password provided—skipping auto-populating placeholders with DB user info.");
            }

            // 2) Load the SMTP config
            var (server, port, useSsl) = GetSmtpConfiguration();

            // 3) Override recipients in dev environment if needed
            OverrideRecipientForDevelopment(emailInput);

            // 4) Build the MimeMessage with placeholders
            var message = CreateEmailMessage(emailInput);

            // 5) Send the email via SMTP
            using var client = new SmtpClient();
            try
            {
                _logger.LogInformation("Connecting to SMTP server: {Server}:{Port}", server, port);
                await client.ConnectAsync(server, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                if (string.IsNullOrWhiteSpace(emailInput.UserName) || string.IsNullOrWhiteSpace(emailInput.Password))
                {
                    throw new ArgumentNullException("Username and Password must be provided.");
                }

                _logger.LogInformation("Authenticating SMTP client as {UserName}", emailInput.UserName);
                await client.AuthenticateAsync($"{emailInput.UserName}@airway.com", LoginUtils.DecryptPassword(emailInput.Password));

                _logger.LogInformation("Sending email to {ToEmail}", string.Join(", ", emailInput.ToEmails));
                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while sending email: {Message}", ex.Message);
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server.");
                }
            }
        }


        public async Task<(string FullName, string Email, string JobTitle, string DirectPhone, string MobilePhone)> GetSenderInfoAsync(string username)
        {
            var normalizedUserName = username.Trim().ToLower();
            var senderInfo = await _context.Users
                .Where(user => user.Uname != null && user.Uname.Trim().ToLower() ==
                    (normalizedUserName == "lvonder" ? "lvonderporten" : normalizedUserName))
                .Select(user => new
                {
                    FullName = $"{user.Fname} {user.Lname}".Trim(),
                    user.Email,
                    user.JobTitle,
                    user.DirectPhone,
                    user.MobilePhone
                })
                .FirstOrDefaultAsync();

            if (senderInfo == null)
            {
                _logger.LogWarning("Sender information not found for {Username}", username);
                return ("System", $"{username}@airway.com", "N/A", "N/A", "N/A");
            }

            return (senderInfo.FullName, senderInfo.Email, senderInfo.JobTitle ?? "N/A", senderInfo.DirectPhone ?? "N/A", senderInfo.MobilePhone ?? "N/A");
        }

        private static MimeMessage CreateEmailMessage(EmailInputBase emailInput)
        {
            var message = new MimeMessage
            {
                Subject = emailInput.Subject
            };

            message.From.Add(new MailboxAddress("Airway", emailInput.FromEmail));

            foreach (var toEmail in emailInput.ToEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                message.To.Add(MailboxAddress.Parse(toEmail));
            }

            if (emailInput.CCEmails != null)
            {
                foreach (var ccEmail in emailInput.CCEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    message.Cc.Add(MailboxAddress.Parse(ccEmail));
                }
            }

            var bodyBuilder = new BodyBuilder();
            string emailContent = Email.EmailTemplate;

            // Replace placeholders
            if (emailInput.Placeholders != null)
            {
                emailContent = ReplacePlaceholders(emailContent, emailInput.Placeholders);
            }

            // Add attachments
            if (emailInput.Attachments != null)
            {
                foreach (var attachmentPath in emailInput.Attachments.Where(File.Exists))
                {
                    bodyBuilder.Attachments.Add(attachmentPath);
                }
            }

            // Add linked images
            string[] logos = ["image1.png", "image2.png", "image3.png", "image4.png", "image5.png"];
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Logos");

            for (int i = 0; i < logos.Length; ++i)
            {
                var fullPathToLogo = Path.Combine(logoPath, logos[i]);
                if (File.Exists(fullPathToLogo))
                {
                    var image = bodyBuilder.LinkedResources.Add(fullPathToLogo);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    emailContent = emailContent.Replace($"%%IMAGE{i + 1}%%", $"cid:{image.ContentId}");
                }
            }

            bodyBuilder.HtmlBody = ApplyEmailStyling(emailContent);
            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private static string ReplacePlaceholders(string emailBody, IDictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
            {
                emailBody = emailBody.Replace(placeholder.Key, placeholder.Value);
            }
            return emailBody;
        }

        private static string ApplyEmailStyling(string bodyContent)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.5; }}
                    .email-body {{ padding: 20px; }}
                </style>
            </head>
            <body class='email-body'>
                {bodyContent}
            </body>
            </html>";
        }

        private void OverrideRecipientForDevelopment(EmailInputBase emailInput)
        {
            if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var developerEmail = GetDeveloperEmail();
                _logger.LogWarning("Development mode: Overriding all recipients to {CurrentUserEmail}", developerEmail);

                emailInput.ToEmails = new List<string> { developerEmail };
                emailInput.CCEmails = new List<string>();
            }
        }

        private string GetDeveloperEmail()
        {
            return _configuration.GetValue<string>("EmailSettings:DevEmail") ?? "ccreech@airway.com";
        }

        private (string Server, int Port, bool UseSsl) GetSmtpConfiguration()
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            return (
                Server: emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SMTP Server is not configured."),
                Port: emailSettings.GetValue<int>("SmtpPort"),
                UseSsl: emailSettings.GetValue<bool>("EnableSsl")
            );
        }
    }
}

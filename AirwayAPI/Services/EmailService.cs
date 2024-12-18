using AirwayAPI.Application;
using AirwayAPI.Assets;
using AirwayAPI.Models.EmailModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;

namespace AirwayAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailInputBase emailInput)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail}", emailInput.ToEmails);

            var (server, port, useSsl) = GetSmtpConfiguration();

            // Override recipients in development environment
            OverrideRecipientForDevelopment(emailInput);

            var message = CreateEmailMessage(emailInput);

            using var client = new SmtpClient();

            try
            {
                // Connect to SMTP server
                _logger.LogInformation("Connecting to SMTP server: {Server}:{Port}", server, port);
                await client.ConnectAsync(server, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                // Authenticate with SMTP server
                if (string.IsNullOrWhiteSpace(emailInput.UserName) || string.IsNullOrWhiteSpace(emailInput.Password))
                {
                    throw new ArgumentNullException("Username and Password must be provided.");
                }

                _logger.LogInformation("Authenticating SMTP client as {UserName}", emailInput.UserName);
                await client.AuthenticateAsync($"{emailInput.UserName}@airway.com", LoginUtils.DecryptPassword(emailInput.Password));

                // Send the email
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

        private void OverrideRecipientForDevelopment(EmailInputBase emailInput)
        {
            if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var currentUserEmail = GetCurrentUserEmail();
                _logger.LogWarning("Development mode: Overriding all recipients to {CurrentUserEmail}", currentUserEmail);

                // Replace recipients with the current user's email
                emailInput.ToEmails = new List<string> { currentUserEmail };
                emailInput.CCEmails = new List<string>();
            }
        }

        private string GetCurrentUserEmail()
        {
            return _configuration.GetValue<string>("DevEmail") ?? "ccreech@airway.com";
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
    }
}

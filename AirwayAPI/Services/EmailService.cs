using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models.ServiceModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Security.Claims;

namespace AirwayAPI.Services
{
    public class EmailService(
        ILogger<EmailService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        private readonly ILogger<EmailService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // Centralized method to send emails
        public async Task SendEmailAsync(EmailInput emailInput)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail}", emailInput.ToEmail);

            // Retrieve SMTP configuration
            var (smtpServer, smtpPort, enableSsl) = GetSmtpConfiguration();

            // Override recipient if in development mode
            OverrideRecipientForDevelopment(emailInput);

            // Use the provided fromEmail or default to the current user's email
            emailInput.FromEmail ??= GetCurrentUserEmail();

            using var client = new SmtpClient();
            ConfigureSmtpClient(client, enableSsl);

            try
            {
                await AuthenticateSmtpClient(client, emailInput.Password);
                var message = CreateEmailMessage(emailInput);

                // Send the email
                _logger.LogInformation("Sending email to {ToEmail}", emailInput.ToEmail);
                await client.SendAsync(message);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
            finally
            {
                await DisconnectSmtpClient(client);
            }
        }

        // Retrieves SMTP server configuration from appsettings
        private (string smtpServer, int smtpPort, bool enableSsl) GetSmtpConfiguration()
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string smtpServer = emailSettings.GetValue<string>("SmtpServer") ?? throw new InvalidOperationException("SMTP Server is not configured.");
            int smtpPort = emailSettings.GetValue<int>("SmtpPort");
            bool enableSsl = emailSettings.GetValue<bool>("EnableSsl");

            return (smtpServer, smtpPort, enableSsl);
        }

        // Override email recipient for development purposes
        private void OverrideRecipientForDevelopment(EmailInput emailInput)
        {
            if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                string currentUserEmail = GetCurrentUserEmail();
                emailInput.ToEmail = currentUserEmail;
                emailInput.CCEmails = [];
                _logger.LogInformation("In development mode: overriding recipient email to current user: {CurrentUserEmail}", currentUserEmail);
            }
        }

        // Configures the SMTP client
        private static void ConfigureSmtpClient(SmtpClient client, bool enableSsl)
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Handle SSL certificate validation
        }

        // Authenticates the SMTP client
        private async Task AuthenticateSmtpClient(SmtpClient client, string password)
        {
            string currentUserEmail = GetCurrentUserEmail();
            string decryptedPassword = LoginUtils.DecryptPassword(password);
            _logger.LogInformation("Authenticating as {UserEmail}", currentUserEmail);
            await client.ConnectAsync(GetSmtpConfiguration().smtpServer, GetSmtpConfiguration().smtpPort, GetSmtpConfiguration().enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await client.AuthenticateAsync(currentUserEmail, decryptedPassword);
        }

        // Creates the email message
        private static MimeMessage CreateEmailMessage(EmailInput emailInput)
        {
            var message = new MimeMessage
            {
                From = { new MailboxAddress("Airway", emailInput.FromEmail) },
                To = { MailboxAddress.Parse(emailInput.ToEmail) },
                Subject = emailInput.Subject
            };

            // Add CC recipients
            if (emailInput.CCEmails != null)
            {
                foreach (var ccEmail in emailInput.CCEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    message.Cc.Add(MailboxAddress.Parse(ccEmail));
                }
            }

            // Set email priority if marked as urgent
            if (emailInput.Urgent)
            {
                message.Headers.Add("X-Priority", "1"); // Highest priority
                message.Headers.Add("X-MSMail-Priority", "High");
                message.Headers.Add("Importance", "High");
            }

            // Build the email body with styling
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = ApplyEmailStyling(emailInput.HtmlBody)
            };

            // Add attachments if any
            if (emailInput.Attachments != null)
            {
                foreach (var attachmentPath in emailInput.Attachments.Where(path => !string.IsNullOrWhiteSpace(path)))
                {
                    bodyBuilder.Attachments.Add(attachmentPath);
                }
            }

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        // Disconnects the SMTP client
        private async Task DisconnectSmtpClient(SmtpClient client)
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
                _logger.LogInformation("SMTP connection closed.");
            }
        }

        // Applies modern styling to the email body
        private static string ApplyEmailStyling(string bodyContent)
        {
            return $@"
                <html>
                <head>
                    <style>
                        .card {{
                            max-width: 600px;
                            margin: auto;
                            padding: 20px;
                            border: 1px solid #e0e0e0;
                            border-radius: 8px;
                            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                            font-family: 'Roboto', 'Helvetica', 'Arial', sans-serif;
                            color: #333;
                        }}
                        .card h2 {{
                            font-size: 24px;
                            font-weight: 400;
                            margin-bottom: 20px;
                            text-align: center;
                            color: #3f51b5;
                        }}
                        .card p {{
                            font-size: 16px;
                            line-height: 1.5;
                        }}
                        .card table {{
                            width: 100%;
                            border-collapse: collapse;
                            margin-top: 20px;
                        }}
                        .card th, .card td {{
                            text-align: left;
                            padding: 8px;
                        }}
                        .card th {{
                            background-color: #f5f5f5;
                            border-bottom: 1px solid #ddd;
                        }}
                        .card tr:nth-child(even) {{
                            background-color: #fafafa;
                        }}
                        .card a {{
                            color: #3f51b5;
                            text-decoration: none;
                        }}
                        .card a:hover {{
                            text-decoration: underline;
                        }}
                    </style>
                </head>
                <body>
                    <div class='card'>
                        {bodyContent}
                    </div>
                </body>
                </html>";
        }

        // Gets the current user's email from claims
        private string GetCurrentUserEmail()
        {
            var usernameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (usernameClaim != null && !string.IsNullOrEmpty(usernameClaim.Value))
            {
                string username = usernameClaim.Value;
                string emailAddress = $"{username}@airway.com";
                _logger.LogInformation("Constructed email from username: {Email}", emailAddress);
                return emailAddress;
            }
            else
            {
                _logger.LogError("Current user's username not found in claims.");
                throw new InvalidOperationException("User's username claim is missing.");
            }
        }
    }
}

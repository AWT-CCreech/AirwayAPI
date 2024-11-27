using AirwayAPI.Models.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Security.Claims;

namespace AirwayAPI.Services
{
    public class EmailService(
        ILogger<EmailService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor) : IEmailService
    {
        private readonly ILogger<EmailService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task SendEmailAsync(EmailInputDto emailInput)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail}", emailInput.ToEmail);

            var (smtpServer, smtpPort, enableSsl) = GetSmtpConfiguration();

            OverrideRecipientForDevelopment(emailInput);

            emailInput.FromEmail ??= GetCurrentUserEmail();

            using var client = new SmtpClient();
            ConfigureSmtpClient(client, enableSsl);

            try
            {
                await AuthenticateSmtpClient(client);
                var message = CreateEmailMessage(emailInput);

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

        private (string smtpServer, int smtpPort, bool enableSsl) GetSmtpConfiguration()
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string smtpServer = emailSettings.GetValue<string>("SmtpServer") ?? throw new InvalidOperationException("SMTP Server is not configured.");
            int smtpPort = emailSettings.GetValue<int>("SmtpPort");
            bool enableSsl = emailSettings.GetValue<bool>("EnableSsl");

            return (smtpServer, smtpPort, enableSsl);
        }

        private void OverrideRecipientForDevelopment(EmailInputDto emailInput)
        {
            if (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                string currentUserEmail = GetCurrentUserEmail();
                emailInput.ToEmail = currentUserEmail;
                emailInput.CCEmails = [];
                _logger.LogInformation("In development mode: overriding recipient email to current user: {CurrentUserEmail}", currentUserEmail);
            }
        }

        private static void ConfigureSmtpClient(SmtpClient client, bool enableSsl)
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        private async Task AuthenticateSmtpClient(SmtpClient client)
        {
            string smtpUser = _configuration.GetValue<string>("EmailSettings:SmtpUser") ?? throw new InvalidOperationException("SMTP User is not configured.");
            string smtpPass = _configuration.GetValue<string>("EmailSettings:SmtpPass") ?? throw new InvalidOperationException("SMTP Password is not configured.");

            _logger.LogInformation("Authenticating with SMTP server as {UserEmail}", smtpUser);
            await client.ConnectAsync(GetSmtpConfiguration().smtpServer, GetSmtpConfiguration().smtpPort, GetSmtpConfiguration().enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await client.AuthenticateAsync(smtpUser, smtpPass);
        }

        private MimeMessage CreateEmailMessage(EmailInputDto emailInput)
        {
            var message = new MimeMessage
            {
                From = { new MailboxAddress("Airway", emailInput.FromEmail) },
                To = { MailboxAddress.Parse(emailInput.ToEmail) },
                Subject = emailInput.Subject
            };

            if (emailInput.CCEmails != null && emailInput.CCEmails.Any())
            {
                foreach (var ccEmail in emailInput.CCEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    message.Cc.Add(MailboxAddress.Parse(ccEmail));
                }
            }

            if (emailInput.Urgent)
            {
                message.Headers.Add("X-Priority", "1");
                message.Headers.Add("X-MSMail-Priority", "High");
                message.Headers.Add("Importance", "High");
            }

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = ApplyEmailStyling(emailInput.HtmlBody)
            };

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

        private async Task DisconnectSmtpClient(SmtpClient client)
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
                _logger.LogInformation("SMTP connection closed.");
            }
        }

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

        private string GetCurrentUserEmail()
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
            if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
            {
                _logger.LogInformation("Current user email: {Email}", emailClaim.Value);
                return emailClaim.Value;
            }
            else
            {
                _logger.LogError("Current user's email not found in claims.");
                throw new InvalidOperationException("User's email claim is missing.");
            }
        }
    }
}

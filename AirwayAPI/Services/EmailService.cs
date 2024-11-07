using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Models.ServiceModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;

namespace AirwayAPI.Services
{
    public class EmailService
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(
            eHelpDeskContext context,
            ILogger<EmailService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        // Centralized SendEmailAsync method
        public async Task SendEmailAsync(EmailInput emailInput)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail}", emailInput.ToEmail);

            // Read SMTP configuration from appsettings
            var emailSettings = _configuration.GetSection("EmailSettings");
            string smtpServer = emailSettings.GetValue<string>("SmtpServer");
            int smtpPort = emailSettings.GetValue<int>("SmtpPort");
            bool enableSsl = emailSettings.GetValue<bool>("EnableSsl");
            bool overrideRecipient = emailSettings.GetValue<bool>("OverrideRecipient", false);

            // Override recipient if setting is true
            if (overrideRecipient)
            {
                string currentUserEmail = GetCurrentUserEmail();
                emailInput.ToEmail = currentUserEmail;
                emailInput.CCEmails = null; // Optional: Clear CC emails
                _logger.LogInformation("Overriding recipient email to current user: {CurrentUserEmail}", currentUserEmail);
            }

            using var client = new SmtpClient();

            // Handle SSL certificate validation (adjust as necessary for production)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            try
            {
                // Connect to the SMTP server
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                string userEmail = emailInput.FromEmail;

                // Authenticate with the SMTP server
                _logger.LogInformation("Authenticating as {UserEmail}", userEmail);
                await client.AuthenticateAsync(userEmail, LoginUtils.decryptPassword(emailInput.Password));

                // Create the email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Airway", userEmail));
                message.To.Add(MailboxAddress.Parse(emailInput.ToEmail));

                // Add CC recipients if any
                if (emailInput.CCEmails != null)
                {
                    foreach (var ccEmail in emailInput.CCEmails)
                    {
                        message.Cc.Add(MailboxAddress.Parse(ccEmail));
                    }
                }

                message.Subject = emailInput.Subject;

                // Build the email body with modern styling
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = ApplyEmailStyling(emailInput.HtmlBody)
                };

                // Add attachments if any
                if (emailInput.Attachments != null)
                {
                    foreach (var attachmentPath in emailInput.Attachments)
                    {
                        bodyBuilder.Attachments.Add(attachmentPath);
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Send the email
                _logger.LogInformation("Sending email to {ToEmail}", emailInput.ToEmail);
                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully.");
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "SMTP Authentication failed for {UserEmail}", emailInput.FromEmail);
                throw new InvalidOperationException($"SMTP Authentication failed: {authEx.Message}", authEx);
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP command error while sending email");
                throw new InvalidOperationException($"SMTP command error: {smtpEx.Message}", smtpEx);
            }
            catch (SmtpProtocolException smtpProtocolEx)
            {
                _logger.LogError(smtpProtocolEx, "SMTP protocol error while sending email");
                throw new InvalidOperationException($"SMTP protocol error: {smtpProtocolEx.Message}", smtpProtocolEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
            finally
            {
                // Disconnect from the SMTP server
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("SMTP connection closed.");
                }
            }
        }

        // Helper method to apply modern email styling
        private string ApplyEmailStyling(string bodyContent)
        {
            var styledBody = $@"
                <html>
                <head>
                    <style>
                        /* Your CSS styles */
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
            return styledBody;
        }

        // Method to check conditions and send delivery date email
        public async Task CheckAndSendDeliveryDateEmail(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            _logger.LogInformation("Checking delivery date email conditions for Sales Order {Sonum}", poLogEntry.SalesOrderNum);

            try
            {
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(s => s.RwsalesOrderNum == poLogEntry.SalesOrderNum);

                if (salesOrder == null)
                {
                    _logger.LogWarning("Sales Order not found for SO#{Sonum}", poLogEntry.SalesOrderNum);
                    return;
                }

                DateTime? saleReqDate = salesOrder.RequiredDate;
                if (updateDto.ExpectedDelivery.HasValue && saleReqDate.HasValue &&
                    updateDto.ExpectedDelivery.Value > saleReqDate.Value)
                {
                    if (poLogEntry.DeliveryDateEmail != true)
                    {
                        poLogEntry.DeliveryDateEmail = true;
                        _logger.LogInformation("Preparing email for delayed delivery of Sales Order {Sonum}", poLogEntry.SalesOrderNum);

                        var salesRep = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == salesOrder.AccountMgr);

                        if (salesRep != null && !string.IsNullOrEmpty(salesRep.Email))
                        {
                            var emailInput = new PODetailEmailInput
                            {
                                ToEmail = salesRep.Email,
                                SoNum = poLogEntry.SalesOrderNum ?? "Unknown SO",
                                SalesRep = salesRep.Uname ?? "Sales Representative",
                                CompanyName = salesOrder.ShipToCompanyName ?? "Unknown Company",
                                SalesRequiredDate = saleReqDate.Value.ToShortDateString(),
                                ExpectedDeliveryDate = updateDto.ExpectedDelivery.Value.ToShortDateString(),
                                PartNumber = poLogEntry.ItemNum ?? "Unknown Part",
                                Notes = updateDto.NewNote ?? "",
                                Urgent = updateDto.UrgentEmail,
                                Subject = updateDto.UrgentEmail
                                    ? $"*** PO#{updateDto.PONum} FOR {salesOrder.ShipToCompanyName} IS DELAYED BY {(updateDto.ExpectedDelivery.Value - saleReqDate.Value).Days} DAYS ***"
                                    : $"PO Delivery Date Exceeds Required Date for SO#{poLogEntry.SalesOrderNum}"
                            };

                            _logger.LogInformation("Sending delay notification email to {ToEmail}", salesRep.Email);
                            await PODetailUpdateSendEmail(emailInput, updateDto);
                        }
                        else
                        {
                            _logger.LogWarning("Sales representative email not found for Sales Order {Sonum}", poLogEntry.SalesOrderNum);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and sending delivery date email for Sales Order {Sonum}", poLogEntry.SalesOrderNum);
                throw new InvalidOperationException("Error checking and sending delivery date email.", ex);
            }
        }

        private async Task PODetailUpdateSendEmail(PODetailEmailInput emailInput, PODetailUpdateDto updateDto)
        {
            _logger.LogInformation("Attempting to send email for Sales Order {Sonum} to {ToEmail}", emailInput.SoNum, emailInput.ToEmail);

            try
            {
                // Prepare the email body content
                var emailBody = $@"
                    <h2>{emailInput.Subject}</h2>
                    <p>The expected delivery date for the following sales order exceeds the required date:</p>
                    <table>
                        <tr><th>Sales Order Number</th><td>{emailInput.SoNum}</td></tr>
                        <tr><th>Company Name</th><td>{emailInput.CompanyName}</td></tr>
                        <tr><th>Sales Required Date</th><td>{emailInput.SalesRequiredDate}</td></tr>
                        <tr><th>Expected Delivery Date</th><td>{emailInput.ExpectedDeliveryDate}</td></tr>
                        <tr><th>Part Number</th><td>{emailInput.PartNumber}</td></tr>
                        {(string.IsNullOrWhiteSpace(emailInput.Notes) ? "" : $"<tr><th>New Note</th><td>{emailInput.Notes}</td></tr>")}
                    </table>";

                // Create an instance of EmailInput for the centralized method
                var email = new EmailInput
                {
                    FromEmail = GetSenderEmail(updateDto.UserName),
                    ToEmail = emailInput.ToEmail,
                    Subject = emailInput.Subject,
                    HtmlBody = emailBody,
                    UserName = updateDto.UserName,
                    Password = updateDto.Password,
                    CCEmails = null, // Add CC emails if needed
                    Attachments = null // Add attachments if needed
                };

                // Use the centralized SendEmailAsync method
                await SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }

        // Helper method to get sender email address
        private string GetSenderEmail(string userName)
        {
            string lowerUserName = userName.Trim().ToLower();
            if (lowerUserName == "lvonderporten")
                return "lvonder@airway.com";
            else
                return $"{lowerUserName}@airway.com";
        }

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

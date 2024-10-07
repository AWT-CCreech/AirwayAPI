using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PODeliveryLogModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace AirwayAPI.Services
{
    public class EmailService
    {
        private readonly eHelpDeskContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmailService> _logger;

        public EmailService(eHelpDeskContext context, IHttpContextAccessor httpContextAccessor, ILogger<EmailService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task CheckAndSendDeliveryDateEmail(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            _logger.LogInformation("Checking delivery date email conditions for Sales Order {SalesOrderNum}", poLogEntry.SalesOrderNum);

            try
            {
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(s => s.RwsalesOrderNum == poLogEntry.SalesOrderNum);

                if (salesOrder != null)
                {
                    DateTime? saleReqDate = salesOrder.RequiredDate;
                    if (updateDto.ExpectedDelivery.HasValue && saleReqDate.HasValue &&
                        updateDto.ExpectedDelivery.Value > saleReqDate.Value)
                    {
                        bool emailSent = poLogEntry.DeliveryDateEmail == true;

                        if (!emailSent)
                        {
                            poLogEntry.DeliveryDateEmail = true;
                            _logger.LogInformation("Preparing email for delayed delivery of Sales Order {SalesOrderNum}", poLogEntry.SalesOrderNum);

                            var salesRep = await _context.Users
                                .FirstOrDefaultAsync(u => u.Uname == salesOrder.EnteredBy);

                            if (salesRep != null && salesRep.Email != null)
                            {
                                try
                                {
                                    var emailInput = new PODetailEmailInput
                                    {
                                        ToEmail = salesRep.Email,
                                        SalesOrderNum = poLogEntry.SalesOrderNum!,
                                        CompanyName = salesOrder.ShipToCompanyName!,
                                        SalesRequiredDate = saleReqDate.Value.ToShortDateString(),
                                        DeliveryDate = updateDto.ExpectedDelivery.Value.ToShortDateString(),
                                        PartNumber = poLogEntry.ItemNum!,
                                        Notes = updateDto.NewNote ?? "",
                                        Urgent = updateDto.UrgentEmail,
                                        Subject = updateDto.UrgentEmail
                                            ? $"*** YOUR PO FOR {salesOrder.ShipToCompanyName} IS DELAYED BY {(updateDto.ExpectedDelivery.Value - saleReqDate.Value).Days} DAYS ***"
                                            : $"The PO Delivery Date Exceeds the Sales Required Date for Sales Order # {poLogEntry.SalesOrderNum}"
                                    };

                                    _logger.LogInformation("Sending delay notification email to {ToEmail}", salesRep.Email);
                                    await PODetailUpdateSendEmail(emailInput, updateDto);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error occurred while preparing email data for Sales Order {SalesOrderNum}", poLogEntry.SalesOrderNum);
                                    throw new InvalidOperationException("Error preparing email data.", ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and sending delivery date email for Sales Order {SalesOrderNum}", poLogEntry.SalesOrderNum);
                throw new InvalidOperationException("Error checking and sending delivery date email.", ex);
            }
        }

        private async Task PODetailUpdateSendEmail(PODetailEmailInput emailInput, PODetailUpdateDto updateDto)
        {
            _logger.LogInformation("Attempting to send email for Sales Order {SalesOrderNum} to {ToEmail}", emailInput.SalesOrderNum, emailInput.ToEmail);

            try
            {
                using var client = new SmtpClient();

                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                _logger.LogDebug("Connecting to SMTP server...");

                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                _logger.LogInformation("Connected to SMTP server.");

                string userEmail;
                if (updateDto.UserName.Trim().ToLower() == "lvonderporten")
                {
                    userEmail = "lvonder@airway.com";
                }
                else
                {
                    userEmail = updateDto.UserName.Trim().ToLower() + "@airway.com";
                }

                try
                {
                    _logger.LogInformation("Authenticating as {UserEmail}", userEmail);
                    await client.AuthenticateAsync(userEmail, LoginUtils.decryptPassword(updateDto.Password));
                }
                catch (AuthenticationException authEx)
                {
                    _logger.LogError(authEx, "SMTP Authentication failed for {UserEmail}", userEmail);
                    throw new InvalidOperationException($"SMTP Authentication failed: {authEx.Message}");
                }

                var httpContext = _httpContextAccessor.HttpContext;
                bool isLocalhost = httpContext!.Request.Host.Host.ToLower() == "localhost" && httpContext.Request.Host.Port == 5001;
                var message = new MimeMessage
                {
                    From = { new MailboxAddress("Purchasing Department", userEmail) },
                    Subject = emailInput.Subject
                };

                if (isLocalhost)
                {
                    message.To.Add(new MailboxAddress("Chris Creech", userEmail));
                    _logger.LogInformation("Sending email to Chris Creech (localhost mode)");
                }
                else
                {
                    message.To.Add(new MailboxAddress("Sales Representative", emailInput.ToEmail));
                    _logger.LogInformation("Sending email to Sales Representative {ToEmail}", emailInput.ToEmail);
                }

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <html>
                    <body>
                        <div>Sales Order #: {emailInput.SalesOrderNum}</div>
                        <div>Sales Required Date: {emailInput.SalesRequiredDate}</div>
                        <div>Delivery Date: {emailInput.DeliveryDate}</div>
                        <div>Part #: {emailInput.PartNumber}</div>
                        <div>{(string.IsNullOrWhiteSpace(emailInput.Notes) ? "" : $"NewNote: {emailInput.Notes}")}</div>
                    </body>
                    </html>"
                };

                if (updateDto.UrgentEmail)
                {
                    message.Subject = $"*** YOUR PO FOR {emailInput.CompanyName} IS DELAYED BY {(DateTime.Parse(emailInput.DeliveryDate) - DateTime.Parse(emailInput.SalesRequiredDate)).Days} DAYS ***";
                }

                message.Body = bodyBuilder.ToMessageBody();

                try
                {
                    _logger.LogInformation("Sending email for Sales Order {SalesOrderNum} to {ToEmail}", emailInput.SalesOrderNum, emailInput.ToEmail);
                    await client.SendAsync(message);
                }
                catch (SmtpCommandException smtpEx)
                {
                    _logger.LogError(smtpEx, "SMTP command error while sending email");
                    throw new InvalidOperationException($"SMTP command error: {smtpEx.Message}");
                }
                catch (SmtpProtocolException smtpProtocolEx)
                {
                    _logger.LogError(smtpProtocolEx, "SMTP protocol error while sending email");
                    throw new InvalidOperationException($"SMTP protocol error: {smtpProtocolEx.Message}");
                }

                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully and SMTP connection closed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }
    }
}

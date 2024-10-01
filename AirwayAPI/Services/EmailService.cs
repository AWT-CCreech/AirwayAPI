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
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(eHelpDeskContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        private async Task PODetailUpdateSendEmail(PODetailEmailInput emailInput, PODetailUpdateDto updateDto)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    // Enable detailed logging (for debugging)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                    // Authenticate user for sending email
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
                        await client.AuthenticateAsync(userEmail, LoginUtils.decryptPassword(updateDto.Password));
                    }
                    catch (AuthenticationException authEx)
                    {
                        throw new InvalidOperationException($"SMTP Authentication failed: {authEx.Message}");
                    }

                    var httpContext = _httpContextAccessor.HttpContext;
                    bool isLocalhost = httpContext!.Request.Host.Host.ToLower() == "localhost" && httpContext.Request.Host.Port == 5001;
                    var message = new MimeMessage
                    {
                        From = { new MailboxAddress("Purchasing Department", userEmail) },
                        Subject = emailInput.Subject
                    };

                    // Check if the current host is localhost:3000
                    if (isLocalhost)
                    {
                        // If localhost, send to Chris Creech
                        message.To.Add(new MailboxAddress("Chris Creech", userEmail));
                    }
                    else
                    {
                        // Otherwise, send to the sales representative
                        message.To.Add(new MailboxAddress("Sales Representative", emailInput.ToEmail));
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
                            <div>{(string.IsNullOrWhiteSpace(emailInput.Notes) ? "" : $"Notes: {emailInput.Notes}")}</div>
                        </body>
                        </html>"
                    };

                    message.Body = bodyBuilder.ToMessageBody();

                    try
                    {
                        // Send email
                        await client.SendAsync(message);
                    }
                    catch (SmtpCommandException smtpEx)
                    {
                        throw new InvalidOperationException($"SMTP command error: {smtpEx.Message}");
                    }
                    catch (SmtpProtocolException smtpProtocolEx)
                    {
                        throw new InvalidOperationException($"SMTP protocol error: {smtpProtocolEx.Message}");
                    }

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }


        public async Task CheckAndSendDeliveryDateEmail(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
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

                            var salesRep = await _context.Users
                                .FirstOrDefaultAsync(u => u.Uname == salesOrder.EnteredBy);

                            if (salesRep != null)
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
                                        Notes = updateDto.Notes ?? "",
                                        Urgent = updateDto.UrgentEmail,
                                        Subject = updateDto.UrgentEmail
                                            ? $"*** YOUR PO FOR {salesOrder.ShipToCompanyName} IS DELAYED BY {(updateDto.ExpectedDelivery.Value - saleReqDate.Value).Days} DAYS ***"
                                            : $"The PO Delivery Date Exceeds the Sales Required Date for Sales Order # {poLogEntry.SalesOrderNum}"
                                    };

                                    await PODetailUpdateSendEmail(emailInput, updateDto);
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidOperationException("Error preparing email data.", ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error checking and sending delivery date email.", ex);
            }
        }

    }
}

using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DropShipModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace AirwayAPI.Controllers.DropShipControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DropShipSendEmailController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        [HttpPost]
        public async Task<ActionResult> DropShipSendEmailAsync([FromBody] DropShipEmailInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.UserName) || string.IsNullOrWhiteSpace(input.Password) ||
                input.ToEmails == null || input.RecipientNames == null)
            {
                return BadRequest("Invalid input.");
            }

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    if (input.UserName.Trim().ToLower() == "lvonderporten")
                    {
                        await client.AuthenticateAsync("lvonder@airway.com", LoginUtils.DecryptPassword(input.Password));
                    }
                    else
                    {
                        await client.AuthenticateAsync(input.UserName.Trim().ToLower() + "@airway.com", LoginUtils.DecryptPassword(input.Password));
                    }

                    User? senderInfo;
                    if (input.UserName.Trim().ToLower() == "lvonder")
                    {
                        senderInfo = await _context.Users
                            .Where(user => user.Uname != null && user.Uname.Trim().ToLower() == "lvonderporten")
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        senderInfo = await _context.Users
                            .Where(user => user.Uname != null && user.Uname.Trim().ToLower() == input.UserName.Trim().ToLower())
                            .FirstOrDefaultAsync();
                    }

                    if (senderInfo == null)
                    {
                        return NotFound("Sender user not found.");
                    }

                    string senderFullname = senderInfo.Fname + " " + senderInfo.Lname;
                    if (input.UserName.Trim().ToLower() == "lvonderporten")
                    {
                        senderFullname = "Linda Von der Porten";
                    }

                    var message = new MimeMessage();

                    // Sender profile
                    message.From.Add(new MailboxAddress(senderFullname, input.UserName.Trim().ToLower() == "lvonderporten" ? "lvonder@airway.com" : input.UserName.Trim().ToLower() + "@airway.com"));

                    // Check if running on localhost:5001
                    bool isLocalhost = HttpContext.Request.Host.Host.ToLower() == "localhost" && HttpContext.Request.Host.Port == 5001;

                    if (isLocalhost)
                    {
                        // Only send to current user
                        message.To.Add(new MailboxAddress(senderFullname, input.UserName.Trim().ToLower() == "lvonderporten" ? "lvonder@airway.com" : input.UserName.Trim().ToLower() + "@airway.com"));
                    }
                    else
                    {
                        // Add all recipients
                        for (int i = 0; i < input.ToEmails.Count; ++i)
                        {
                            if (!string.IsNullOrWhiteSpace(input.ToEmails[i]) && !string.IsNullOrWhiteSpace(input.RecipientNames[i]))
                            {
                                message.To.Add(new MailboxAddress(input.RecipientNames[i].Trim(), input.ToEmails[i].Trim()));
                            }
                        }
                        message.To.Add(new MailboxAddress("AirWay Accounts Payable", "airwayap@airway.com"));
                        message.To.Add(new MailboxAddress("Receiving", "Receiving@airway.com"));
                        message.To.Add(new MailboxAddress("Shipping Team", "ShippingTeam@airway.com"));
                        message.To.Add(new MailboxAddress("Purch_Dept", "Purch_Dept@airway.com"));

                        // CC to Ben for now
                        message.Cc.Add(new MailboxAddress("Ben Bleser", "bbleser@airway.com"));
                    }

                    // Set email subject
                    message.Subject = input.Subject;

                    // Create the email body with styling
                    var bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 80%;
                                        margin: auto;
                                        padding: 20px;
                                        border: 1px solid #ccc;
                                        border-radius: 10px;
                                    }}
                                    .header {{
                                        font-size: 1.5em;
                                        margin-bottom: 5px;
                                    }}
                                    .subheader {{
                                        font-size: 1em;
                                        margin-bottom: 20px;
                                    }}
                                    .details {{
                                        margin-bottom: 20px;
                                    }}
                                    .details th, .details td {{
                                        padding: 10px;
                                        border: 1px solid #ddd;
                                        text-align: left;
                                    }}
                                    .footer {{
                                        margin-top: 20px;
                                        font-size: 0.9em;
                                        color: #555;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>This drop ship has been completed.</div>
                                    <div class='subheader'>Please see the details below:</div>
                                    <table class='details'>
                                        <tr><th>PO#</th><td>{input.PONumber}</td></tr>
                                        <tr><th>SO#</th><td>{input.SONumber}</td></tr>
                                        <tr><th>P/N</th><td>{input.PartNumber}</td></tr>
                                        <tr><th>Quantity</th><td>{input.Quantity}</td></tr>
                                        {(string.IsNullOrWhiteSpace(input.SerialNumber) ? "" : $"<tr><th>Serial Number</th><td>{input.SerialNumber}</td></tr>")}
                                        {(string.IsNullOrWhiteSpace(input.Tracking) ? "" : $"<tr><th>Tracking</th><td>{input.Tracking}</td></tr>")}
                                        {(string.IsNullOrWhiteSpace(input.Freight) ? "" : $"<tr><th>Freight</th><td>{(input.Freight.Contains('$') ? input.Freight : "$" + input.Freight)}</td></tr>")}
                                    </table>
                                    <div class='footer'>
                                        Thank you!
                                    </div>
                                </div>
                            </body>
                            </html>"
                    };
                    message.Body = bodyBuilder.ToMessageBody();

                    // Send email
                    await client.SendAsync(message);

                    // Close connection after the email is sent
                    await client.DisconnectAsync(true);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + " ......... " + ex.StackTrace);
            }
        }
    }
}

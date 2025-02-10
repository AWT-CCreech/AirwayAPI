using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Models.MassMailerModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerEmailOutsController(
        IEmailService emailService,
        ILogger<MassMailerEmailOutsController> logger,
        eHelpDeskContext context) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<MassMailerEmailOutsController> _logger = logger;
        private readonly eHelpDeskContext _context = context;

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] MassMailerEmailInput input)
        {
            try
            {
                // Normalize and fetch sender info
                var normalizedUserName = input.UserName.Trim().ToLower();
                var senderInfo = await _context.Users
                    .Where(user => user.Uname != null && user.Uname.Trim().ToLower() ==
                        (normalizedUserName == "lvonder" ? "lvonderporten" : normalizedUserName))
                    .FirstOrDefaultAsync();

                if (senderInfo == null)
                {
                    return NotFound("Sender information not found.");
                }

                // Construct sender details
                string senderFullName = (senderInfo.Fname ?? string.Empty) + " " + (senderInfo.Lname ?? string.Empty);
                if (normalizedUserName == "lvonderporten")
                {
                    senderFullName = "Linda Von der Porten";
                }

                // Generate a combined part table content (for use in the email template)
                // This is optional if you want a summary table in the email body.
                var partTable = "<table><thead><tr><th>AWT P/N</th><th>MFG P/N</th><th>Part Description</th><th>Qty</th><th>Manufacturer</th><th>Rev</th></tr></thead><tbody>";
                foreach (var item in input.Items)
                {
                    partTable += $"<tr><td>{item.PartNum}</td><td>{item.AltPartNum}</td><td>{item.PartDesc}</td><td>{item.Qty}</td><td>{item.Manufacturer}</td><td>{item.Revision}</td></tr>";
                }
                partTable += "</tbody></table>";

                // Generate shared placeholders (used in the email body template)
                var placeholders = new Dictionary<string, string>
                {
                    { "%%EMAILBODY%%", input.Body },
                    { "%%FIRSTNAME%%", senderInfo.Fname ?? string.Empty },
                    { "%%LASTNAME%%", senderInfo.Lname ?? string.Empty },
                    { "%%FULLNAME%%", senderFullName },
                    { "%%JOBTITLE%%", senderInfo.JobTitle ?? string.Empty },
                    { "%%DIRECT%%", senderInfo.DirectPhone ?? string.Empty },
                    { "%%MOBILE%%", senderInfo.MobilePhone ?? string.Empty },
                    { "%%PARTTABLE%%", partTable }
                };

                // Prepare attachment paths
                var folderName = Path.Combine("Files", "MassMailerAttachment", normalizedUserName);
                var attachmentPaths = input.Attachments?
                    .Select(fileName => Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName))
                    .ToList();

                // Sanitize subject once for logging (using legacy stored procedure)
                var sanitizedSubject = input.Subject.Replace("'", "''");

                // Loop through each vendor email (using a for-loop to maintain index for related data)
                for (int i = 0; i < input.ToEmails.Count; i++)
                {
                    var vendorEmail = input.ToEmails[i];

                    // Build email input for the current vendor (sending one email per vendor)
                    var emailInput = new EmailInputBase
                    {
                        FromEmail = $"{input.UserName}@airway.com",
                        ToEmails = new List<string> { vendorEmail },
                        CCEmails = input.CCEmails, // these remain common for all emails
                        Subject = input.Subject,
                        Body = input.Body,
                        Attachments = attachmentPaths,
                        InlineImages = new List<string>(),
                        UserName = input.UserName,
                        Password = input.Password,
                        Placeholders = placeholders
                    };

                    // Send the email
                    await _emailService.SendEmailAsync(emailInput);
                    _logger.LogInformation("Email sent to vendor: {VendorEmail}", vendorEmail);

                    // Log the email send via the legacy stored procedure (which inserts into MassMailer table)
                    var spCommandText = "EXEC usp_ins_MassMailers @MassMailDesc, @DateSent, @UserID, @RequestId OUT";
                    var descParam = new SqlParameter("@MassMailDesc", sanitizedSubject);
                    var dateParam = new SqlParameter("@DateSent", DateTime.Now);
                    var userIdParam = new SqlParameter("@UserID", senderInfo.Id);
                    var massMailIdParam = new SqlParameter
                    {
                        ParameterName = "@RequestId",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };

                    _context.Database.ExecuteSqlRaw(spCommandText, new[] { descParam, dateParam, userIdParam, massMailIdParam });
                    _logger.LogInformation("Logged email send via SP for vendor: {VendorEmail} with MassMailId: {MassMailId}", vendorEmail, massMailIdParam.Value);

                    // Now log history records for each part item in this email
                    foreach (var part in input.Items)
                    {
                        var historyRecord = new MassMailHistory
                        {
                            // Use the MassMailId returned from the stored procedure (if available)
                            MassMailId = (massMailIdParam.Value != DBNull.Value) ? Convert.ToInt32(massMailIdParam.Value) : (int?)null,
                            CompanyName = (input.RecipientCompanies != null && input.RecipientCompanies.Count > i) ? input.RecipientCompanies[i] : null,
                            ContactName = (input.RecipientNames != null && input.RecipientNames.Count > i) ? input.RecipientNames[i] : null,
                            RequestId = part.RequestId,
                            PartNum = part.PartNum,
                            AltPartNum = part.AltPartNum,
                            PartDesc = part.PartDesc,
                            Qty = Convert.ToInt32(part.Qty),
                            DateSent = DateTime.Now,
                            RespondedTo = false
                        };

                        _context.MassMailHistories.Add(historyRecord);
                        _logger.LogInformation("Prepared history record for vendor: {VendorEmail} with Part: {PartNum}", vendorEmail, part.PartNum);
                    }

                    // Save all history records for the current vendor
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Logged history records for vendor: {VendorEmail}", vendorEmail);
                }

                return Ok("Emails sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email: {Message}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

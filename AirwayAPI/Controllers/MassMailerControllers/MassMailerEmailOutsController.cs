using AirwayAPI.Data;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Models.MassMailerModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerEmailOutsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<MassMailerEmailOutsController> _logger;
        private readonly eHelpDeskContext _context;

        public MassMailerEmailOutsController(
            IEmailService emailService,
            ILogger<MassMailerEmailOutsController> logger,
            eHelpDeskContext context)
        {
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

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

                // Log email to database
                var commandText = "EXEC usp_ins_MassMailers @MassMailDesc, @DateSent, @UserID, @Id OUT";
                var sanitizedSubject = input.Subject.Replace("'", "''");
                var desc = new SqlParameter("@MassMailDesc", sanitizedSubject);
                var date = new SqlParameter("@DateSent", DateTime.Now);
                var userId = new SqlParameter("@UserID", senderInfo.Id);
                var massMailId = new SqlParameter
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                _context.Database.ExecuteSqlRaw(commandText, new[] { desc, date, userId, massMailId });

                // Generate part table content
                var partTable = "<table><thead><tr><th>Airway Part Number</th><th>Mfg Part Number</th><th>Part Description</th><th>Qty</th><th>Manufacturer</th><th>Rev</th></tr></thead><tbody>";
                foreach (var item in input.Items)
                {
                    partTable += $"<tr><td>{item.PartNum}</td><td>{item.AltPartNum}</td><td>{item.PartDesc}</td><td>{item.Qty}</td><td>{item.Manufacturer}</td><td>{item.Revision}</td></tr>";
                }
                partTable += "</tbody></table>";

                // Generate placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "%%EMAILBODY%%", input.Body },
                    { "%%NAME%%", senderFullName },
                    { "%%JOBTITLE%%", senderInfo.JobTitle ?? string.Empty },
                    { "%%DIRECT%%", senderInfo.DirectPhone ?? string.Empty },
                    { "%%MOBILE%%", senderInfo.MobilePhone ?? string.Empty },
                    { "%%PARTTABLE%%", partTable }
                };

                // Prepare attachment paths
                var folderName = Path.Combine("Files", "MassMailerAttachment", normalizedUserName);
                var attachmentPaths = input.Attachments?.Select(fileName =>
                    Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName)).ToList();

                var emailInput = new EmailInputBase
                {
                    FromEmail = $"{input.UserName}@airway.com",
                    ToEmails = input.ToEmails,
                    CCEmails = input.CCEmails,
                    Subject = input.Subject,
                    Body = input.Body,
                    Attachments = attachmentPaths,
                    InlineImages = new List<string>(), // Pass additional inline images if required
                    UserName = input.UserName,
                    Password = input.Password,
                    Placeholders = placeholders
                };

                await _emailService.SendEmailAsync(emailInput);

                // Log the mass mail ID
                _logger.LogInformation("MassMailer entry created with ID: {MassMailId}", massMailId.Value);

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email: {Message}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}

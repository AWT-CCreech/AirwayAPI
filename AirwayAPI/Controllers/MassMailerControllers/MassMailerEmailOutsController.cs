using AirwayAPI.Assets;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Application;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerEmailOutsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<MassMailerEmailOutsController> _logger;

        public MassMailerEmailOutsController(eHelpDeskContext context, ILogger<MassMailerEmailOutsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> SendEmailAsync([FromBody] MassMailerEmailInput input)
        {
            try
            {
                SmtpClient client = new();
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                if (input?.SenderUserName == null || input?.Password == null)
                {
                    throw new ArgumentNullException($"{input?.SenderUserName} and {input?.Password} cannot be null");
                }

                string senderUserName = input.SenderUserName.Trim().ToLower();
                string email = senderUserName == "lvonderporten" ? "lvonder@airway.com" : senderUserName + "@airway.com";

                await client.AuthenticateAsync(email, LoginUtils.decryptPassword(input.Password));

                User? senderInfo;
                if (input.SenderUserName.Trim().ToLower() == "lvonder")
                {
                    senderInfo = await _context.Users
                        .Where(user => user.Uname != null && user.Uname.Trim().ToLower() == "lvonderporten")
                        .FirstOrDefaultAsync();
                }
                else
                {
                    var normalizedSenderUserName = input.SenderUserName?.Trim().ToLower();
                    senderInfo = await _context.Users
                        .Where(user => user.Uname != null && user.Uname.Trim().ToLower() == normalizedSenderUserName)
                        .FirstOrDefaultAsync();
                }

                if (senderInfo == null)
                {
                    return NotFound("Sender information not found.");
                }

                string senderFullname = (senderInfo.Fname ?? string.Empty) + " " + (senderInfo.Lname ?? string.Empty);

                if (!string.IsNullOrEmpty(input.SenderUserName) && input.SenderUserName.Trim().ToLower() == "lvonderporten")
                {
                    senderFullname = "Linda Von der Porten";
                }

                MimeMessage message = new();

                if (!string.IsNullOrEmpty(input.SenderUserName))
                {
                    if (input.SenderUserName.Trim().ToLower() == "lvonderporten")
                    {
                        message.From.Add(new MailboxAddress(senderFullname, "lvonder@airway.com"));
                    }
                    else
                    {
                        message.From.Add(new MailboxAddress(senderFullname, input.SenderUserName.Trim().ToLower() + "@airway.com"));
                    }
                }
                else
                {
                    return BadRequest("SenderUserName cannot be null or empty.");
                }

                if (input?.CCEmails == null || input?.CCNames == null)
                {
                    throw new ArgumentNullException($"{input?.CCEmails} and {input?.CCNames} cannot be null");
                }

                if (input.CCEmails.Length != input.CCNames.Length)
                {
                    throw new ArgumentException("CCEmails and CCNames must have the same length");
                }

                for (int j = 0; j < input.CCEmails.Length; ++j)
                {
                    if (!string.IsNullOrWhiteSpace(input.CCEmails[j]) && !string.IsNullOrWhiteSpace(input.CCNames[j]))
                    {
                        message.Cc.Add(new MailboxAddress(input.CCNames[j].Trim(), input.CCEmails[j].Trim()));
                    }
                }

                message.Subject = input.Subject;

                BodyBuilder bodyBuilder = new();

                string emailContent = Email.EmailTemplate;

                emailContent = emailContent.Replace("%%EMAILBODY%%", input.Body)
                                            .Replace("%%NAME%%", senderFullname)
                                            .Replace("%%JOBTITLE%%", senderInfo.JobTitle)
                                            .Replace("%%DIRECT%%", senderInfo.DirectPhone)
                                            .Replace("%%MOBILE%%", senderInfo.MobilePhone);

                var folderName = Path.Combine("Files", "MassMailerAttachment", input.SenderUserName.Trim().ToLower());

                if (input?.AttachFiles == null)
                {
                    throw new ArgumentNullException($"{input?.AttachFiles} cannot be null");
                }

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    throw new ArgumentNullException($"{folderName} cannot be null or empty");
                }

                foreach (string fileName in input.AttachFiles)
                {
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
                        bodyBuilder.Attachments.Add(fullPath);
                    }
                }

                string[] logos = { "image1.png", "image2.png", "image3.png", "image4.png", "image5.png" };
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Logos");

                for (int i = 0; i < logos.Length; ++i)
                {
                    var fullPathToLogo = Path.Combine(logoPath, logos[i]);
                    var image = bodyBuilder.LinkedResources.Add(fullPathToLogo);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    emailContent = emailContent.Replace($"%%IMAGE{i + 1}%%", image.ContentId);
                }

                var commandText = "EXEC usp_ins_MassMailers @MassMailDesc, @DateSent, @UserID, @Id OUT";
                if (input?.Subject == null)
                    throw new ArgumentNullException($"{input?.Subject} cannot be null");
                var sanitizedSubject = input.Subject.Replace("'", "''");
                var desc = new SqlParameter("@MassMailDesc", sanitizedSubject);
                var date = new SqlParameter("@DateSent", DateTime.Now);
                var userId = new SqlParameter("@UserID", senderInfo.Id);
                var massMailId = new SqlParameter { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

                _context.Database.ExecuteSqlRaw(commandText, new[] { desc, date, userId, massMailId });

                if (input?.RecipientEmails == null || input?.items == null || input?.RecipientCompanies == null || input?.RecipientNames == null || input?.RecipientIds == null)
                {
                    throw new ArgumentNullException($"{input?.RecipientEmails}, {input?.items}, {input?.RecipientCompanies}, {input?.RecipientNames}, or {input?.RecipientIds} fields are null");
                }

                for (int i = 0; i < input.RecipientEmails.Length; ++i)
                {
                    if (string.IsNullOrWhiteSpace(input.RecipientEmails[i]) || string.IsNullOrWhiteSpace(input.RecipientCompanies[i]) || string.IsNullOrWhiteSpace(input.RecipientNames[i]))
                    {
                        throw new ArgumentNullException($"{input.RecipientEmails}, {input.RecipientCompanies}, and {input.RecipientNames} cannot contain null or contain whitespace values");
                    }

                    List<MassMailerPartItem> items = new();
                    for (var j = 0; j < input.items.Length; ++j)
                    {
                        var item = input.items[j];
                        if (item == null)
                        {
                            continue;
                        }

                        var check = await (from c in _context.SellOpCompetitors
                                           join r in _context.EquipmentRequests on c.EventId equals r.EventId
                                           join e in _context.RequestEvents on r.EventId equals e.EventId
                                           join cc in _context.CamContacts on e.ContactId equals cc.Id
                                           where r.RequestId == item.Id
                                           && c.Company != null && c.Company.Trim().ToLower() == input.RecipientCompanies[i].Trim().ToLower()
                                           select c.Id).ToListAsync();

                        if (check.Count == 0)
                        {
                            items.Add(item);

                            _context.CompetitorCalls.Add(new CompetitorCall
                            {
                                PartNum = item.PartNum?.Replace("'", "''") ?? string.Empty,
                                MfgPartNum = item.AltPartNum?.Replace("'", "''") ?? string.Empty,
                                CompanyName = input.RecipientCompanies[i].Replace("'", "''"),
                                ContactName = input.RecipientNames[i].Replace("'", "''"),
                                MassMailing = true,
                                EnteredBy = senderInfo.Id,
                                ModifiedBy = senderInfo.Id,
                                NewOrUsed = ""
                            });

                            _context.MassMailHistories.Add(new MassMailHistory
                            {
                                MassMailId = (int)massMailId.Value,
                                CompanyName = input.RecipientCompanies[i].Replace("'", "''"),
                                ContactName = input.RecipientNames[i].Replace("'", "''"),
                                RequestId = item.Id,
                                PartNum = item.PartNum?.Replace("'", "''") ?? string.Empty,
                                AltPartNum = item.AltPartNum?.Replace("'", "''") ?? string.Empty,
                                PartDesc = item.PartDesc?.Replace("'", "''") ?? string.Empty,
                                Qty = (int?)(item.Qty ?? 0),
                                DateSent = DateTime.Now
                            });

                            int callCnt = await _context.CompetitorCalls.Where(c => c.RequestId == item.Id && c.CallType == "Purchasing")
                                .Select(c => c.CallId).CountAsync();
                            if (callCnt < 1)
                            {
                                var er = await _context.EquipmentRequests.FirstOrDefaultAsync(e => e.RequestId == item.Id);
                                if (er != null)
                                {
                                    er.ProcureRep = senderInfo.Id;
                                }
                                else
                                {
                                    return NotFound("EquipmentRequest not found.");
                                }
                            }

                            await _context.SaveChangesAsync();
                        }

                        _context.CamActivities.Add(new CamActivity
                        {
                            ContactId = input.RecipientIds[i],
                            ActivityOwner = input.SenderUserName,
                            ActivityType = "CallOut",
                            ProjectCode = "",
                            Notes = "<b>Mass Mail Sent For:</b> " + input.Subject.Replace("'", "''"),
                            ActivityDate = DateTime.Now,
                            ActivityTime = DateTime.Now,
                            EnteredBy = input.SenderUserName,
                            ModifiedBy = input.SenderUserName,
                            CompletedBy = input.SenderUserName,
                            CompleteDate = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }

                    if (items.Count > 0)
                    {
                        var temp = input.RecipientNames[i].Split(' ');
                        var firstName = temp[0];
                        string lastName = temp.Length >= 2 ? temp[^1] : string.Empty;

                        // Check if running on localhost
                        bool isLocalhost = HttpContext.Request.Host.Host.ToLower() == "localhost";

                        if (isLocalhost)
                        {
                            // Only send to current user
                            message.To.Add(new MailboxAddress(senderFullname, email));
                        }
                        else
                        {
                            message.To.Add(new MailboxAddress(input.RecipientNames[i].Trim(), input.RecipientEmails[i].Trim()));
                        }

                        var partTable = "";
                        items.ForEach(item =>
                        {
                            partTable = partTable + $"<tr><td>{item.PartNum}</td><td>{item.AltPartNum}</td><td>{item.PartDesc}</td><td>{item.Qty}</td>"
                                           + $"<td>{item.Manufacturer}</td><td>{item.Revision}</td></tr>";
                        });

                        bodyBuilder.HtmlBody = emailContent.Replace("%%PARTTABLE%%", partTable)
                                                            .Replace("%fullname%", input.RecipientNames[i])
                                                            .Replace("%firstname%", firstName)
                                                            .Replace("%lastname%", lastName);
                        message.Body = bodyBuilder.ToMessageBody();

                        await client.SendAsync(message);
                    }
                    else
                    {
                        var errorMessage = new MimeMessage();
                        var senderEmail = input.SenderUserName.Trim().ToLower() == "lvonderporten" ? "lvonder@airway.com" : input.SenderUserName.Trim().ToLower() + "@airway.com";
                        errorMessage.From.Add(new MailboxAddress(senderFullname, senderEmail));
                        errorMessage.To.Add(new MailboxAddress(senderFullname, senderEmail));

                        errorMessage.Subject = "Failed to Send Mass Mailer due to empty part table";
                        errorMessage.Body = new TextPart("plain")
                        {
                            Text = $"The email that you are attempting to send to {input.RecipientCompanies[i].Trim()} ({input.RecipientNames[i].Trim()}) has been cancelled due to an empty part table (potentially caused by matching competitors)."
                        };
                        await client.SendAsync(errorMessage);
                    }
                    message.To.Clear();
                }

                await client.DisconnectAsync(true);
                client.Dispose();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}

using AirwayAPI.Application;
using AirwayAPI.Assets;
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.MassMailerModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;
using System.Data;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerEmailOutsController(eHelpDeskContext context, ILogger<MassMailerEmailOutsController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<MassMailerEmailOutsController> _logger = logger;

        [HttpPost]
        public async Task<ActionResult> SendEmailAsync([FromBody] MassMailerEmailInput input)
        {
            try
            {
                SmtpClient client = new();
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                if (string.IsNullOrWhiteSpace(input?.SenderUserName) || string.IsNullOrWhiteSpace(input?.Password))
                {
                    throw new ArgumentNullException($"{input?.SenderUserName} and {input?.Password} cannot be null");
                }

                string senderUserName = input.SenderUserName.Trim().ToLower();
                string email = senderUserName == "lvonderporten" ? "lvonder@airway.com" : senderUserName + "@airway.com";

                await client.AuthenticateAsync(email, LoginUtils.DecryptPassword(input.Password));

                User? senderInfo;
                var normalizedSenderUserName = input.SenderUserName.Trim().ToLower();

                senderInfo = await _context.Users
                    .Where(user => user.Uname != null && user.Uname.Trim().ToLower() == (normalizedSenderUserName == "lvonder" ? "lvonderporten" : normalizedSenderUserName))
                    .FirstOrDefaultAsync();

                if (senderInfo == null)
                {
                    return NotFound("Sender information not found.");
                }

                string senderFullname = (senderInfo.Fname ?? string.Empty) + " " + (senderInfo.Lname ?? string.Empty);
                if (normalizedSenderUserName == "lvonderporten")
                {
                    senderFullname = "Linda Von der Porten";
                }

                MimeMessage message = new();

                message.From.Add(new MailboxAddress(senderFullname, email));

                if (input.CCEmails?.Length != input.CCNames?.Length)
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

                var folderName = Path.Combine("Files", "MassMailerAttachment", normalizedSenderUserName);

                string[] array = input.AttachFiles ?? [];
                for (int i = 0; i < array.Length; i++)
                {
                    string fileName = array[i];
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
                        bodyBuilder.Attachments.Add(fullPath);
                    }
                }

                string[] logos = ["image1.png", "image2.png", "image3.png", "image4.png", "image5.png"];
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Logos");

                for (int i = 0; i < logos.Length; ++i)
                {
                    var fullPathToLogo = Path.Combine(logoPath, logos[i]);
                    var image = bodyBuilder.LinkedResources.Add(fullPathToLogo);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    emailContent = emailContent.Replace($"%%IMAGE{i + 1}%%", image.ContentId);
                }

                var commandText = "EXEC usp_ins_MassMailers @MassMailDesc, @DateSent, @UserID, @Id OUT";
                var sanitizedSubject = input.Subject.Replace("'", "''");
                var desc = new SqlParameter("@MassMailDesc", sanitizedSubject);
                var date = new SqlParameter("@DateSent", DateTime.Now);
                var userId = new SqlParameter("@UserID", senderInfo.Id);
                var massMailId = new SqlParameter { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

                _context.Database.ExecuteSqlRaw(commandText, new[] { desc, date, userId, massMailId });

                for (int i = 0; i < input.RecipientEmails.Length; ++i)
                {
                    if (string.IsNullOrWhiteSpace(input.RecipientEmails[i]) || string.IsNullOrWhiteSpace(input.RecipientCompanies[i]) || string.IsNullOrWhiteSpace(input.RecipientNames[i]))
                    {
                        throw new ArgumentNullException("Recipient fields cannot contain null or whitespace values");
                    }

                    var items = new List<MassMailerPartItem>();

                    for (var j = 0; j < input.items.Length; ++j)
                    {
                        var item = input.items[j];
                        if (item == null) continue;

                        var check = await (from c in _context.SellOpCompetitors
                                           join r in _context.EquipmentRequests on c.EventId equals r.EventId
                                           join e in _context.RequestEvents on r.EventId equals e.EventId
                                           join cc in _context.CamContacts on e.ContactId equals cc.Id
                                           where r.RequestId == item.Id &&
                                                 (c.Company != null && c.Company.Trim().ToLower() == input.RecipientCompanies[i].Trim().ToLower())
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
                                ModifiedBy = senderInfo.Id
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

                            int callCnt = await _context.CompetitorCalls.Where(c => c.RequestId == item.Id && c.CallType == "Purchasing").CountAsync();
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
                            Notes = "<b>Mass Mail Sent For:</b> " + input.Subject.Replace("'", "''"),
                            ActivityDate = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }

                    if (items.Count > 0)
                    {
                        var nameParts = input.RecipientNames[i].Split(' ');
                        var firstName = nameParts[0];
                        var lastName = nameParts.Length >= 2 ? nameParts[^1] : string.Empty;

                        bool isLocalhost = HttpContext.Request.Host.Host.ToLower() == "localhost";

                        if (isLocalhost)
                        {
                            message.To.Add(new MailboxAddress(senderFullname, email));
                        }
                        else
                        {
                            message.To.Add(new MailboxAddress(input.RecipientNames[i].Trim(), input.RecipientEmails[i].Trim()));
                        }

                        var partTable = "";
                        items.ForEach(item =>
                        {
                            partTable += $"<tr><td>{item.PartNum}</td><td>{item.AltPartNum}</td><td>{item.PartDesc}</td><td>{item.Qty}</td><td>{item.Manufacturer}</td><td>{item.Revision}</td></tr>";
                        });

                        bodyBuilder.HtmlBody = emailContent.Replace("%%PARTTABLE%%", partTable)
                                                            .Replace("%fullname%", input.RecipientNames[i])
                                                            .Replace("%firstname%", firstName)
                                                            .Replace("%lastname%", lastName);
                        message.Body = bodyBuilder.ToMessageBody();

                        await client.SendAsync(message);
                    }
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

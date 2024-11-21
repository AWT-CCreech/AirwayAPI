using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Models.ServiceModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODetailController(eHelpDeskContext context, EmailService emailService, ILogger<PODetailController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly EmailService _emailService = emailService;
        private readonly ILogger<PODetailController> _logger = logger;

        // GET: api/PODetail/id/{id}
        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetPODetailByID(int id)
        {
            var poLogEntry = await _context.TrkPologs
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Ponum,
                    p.SalesOrderNum,
                    p.ItemNum,
                    p.QtyOrdered,
                    p.QtyReceived,
                    p.ReceiverNum,
                    p.ExpectedDelivery,
                    p.ContactId,
                    p.IssuedBy,
                    p.DateDelivered,
                    p.EditDate,
                    EditedBy = _context.Users.Where(u => u.Id == p.EditedBy).Select(u => u.Uname).FirstOrDefault(),
                    p.ExpDelEditDate,
                    Notes = _context.TrkPonotes
                        .Where(n => n.Ponum.ToString() == p.Ponum)
                        .OrderByDescending(n => n.EntryDate)
                        .Select(n => new
                        {
                            n.Notes,
                            n.EntryDate,
                            n.EnteredBy
                        }).ToList(),
                    Contact = _context.CamContacts
                        .Where(c => c.Id == p.ContactId)
                        .Select(c => new
                        {
                            c.Contact,
                            c.Company,
                            Phone = !string.IsNullOrEmpty(c.PhoneDirect) ? c.PhoneDirect : c.PhoneMain
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (poLogEntry == null) return NotFound("PO log entry not found.");

            // If ContactId is missing, attempt to retrieve it from RequestPos
            int? contactId = poLogEntry.ContactId;
            if (!contactId.HasValue && poLogEntry.Ponum != null)
            {
                contactId = await GetContactIdIfMissing(poLogEntry.Ponum);
            }

            // Fetch contact details if ContactId is found in RequestPos
            var contactDetails = contactId.HasValue
                ? await _context.CamContacts
                    .Where(c => c.Id == contactId)
                    .Select(c => new { c.Contact, c.Company, Phone = !string.IsNullOrEmpty(c.PhoneDirect) ? c.PhoneDirect : c.PhoneMain })
                    .FirstOrDefaultAsync()
                : null;

            // Map to DTO
            var poDetailDto = new PODetailUpdateDto
            {
                Id = poLogEntry.Id,
                PONum = poLogEntry.Ponum ?? "",
                SONum = poLogEntry.SalesOrderNum ?? "",
                PartNum = poLogEntry.ItemNum ?? "",
                QtyOrdered = poLogEntry.QtyOrdered,
                QtyReceived = poLogEntry.QtyReceived,
                ReceiverNum = poLogEntry.ReceiverNum ?? 0,
                ExpectedDelivery = poLogEntry.ExpectedDelivery,
                ContactID = contactId ?? 0,
                IssuedBy = poLogEntry.IssuedBy ?? "",
                DateDelivered = poLogEntry.DateDelivered,
                EditDate = poLogEntry.EditDate,
                EditedBy = poLogEntry.EditedBy,
                ExpDelEditDate = poLogEntry.ExpDelEditDate ?? "",
                NotesList = poLogEntry.Notes.Select(note => $"{note.EnteredBy}::{note.Notes}::{(note.EntryDate.HasValue ? note.EntryDate.Value.ToShortDateString() : "No Date")}").ToList(),
                ContactName = contactDetails?.Contact ?? "PLEASE UPDATE",
                Company = contactDetails?.Company ?? "",
                Phone = contactDetails?.Phone ?? ""
            };

            return Ok(poDetailDto);
        }

        // PUT: api/PODetail/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePODetail(int id, [FromBody] PODetailUpdateDto updateDto)
        {
            if (id != updateDto.Id) return BadRequest("ID mismatch.");

            var poLogEntry = await _context.TrkPologs.FindAsync(id);
            if (poLogEntry == null) return NotFound("PO log entry not found.");

            // Retrieve SONum if not provided
            if (string.IsNullOrEmpty(updateDto.SONum))
            {
                var retrievedSONum = await _context.EquipmentRequests
                    .Where(r => r.PartNum == poLogEntry.ItemNum)
                    .Join(_context.RequestPos,
                          r => r.RequestId,
                          p => p.RequestId,
                          (r, p) => new { p.Ponum, r.SalesOrderNum })
                    .Where(joined => joined.Ponum == updateDto.PONum)
                    .Select(joined => joined.SalesOrderNum)
                    .FirstOrDefaultAsync();

                updateDto.SONum = retrievedSONum ?? updateDto.SONum;
            }

            // Ensure ExpectedDelivery is valid
            if (!updateDto.ExpectedDelivery.HasValue)
            {
                return BadRequest("Expected Delivery date is invalid or missing.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool expectedDeliveryChanged = poLogEntry.ExpectedDelivery != updateDto.ExpectedDelivery;

                // Update PO Log entry
                UpdatePODetailFields(poLogEntry, updateDto);

                // Additional field updates
                poLogEntry.QtyOrdered = updateDto.QtyOrdered;
                poLogEntry.QtyReceived = updateDto.QtyReceived;
                poLogEntry.ReceiverNum = updateDto.ReceiverNum;

                if (expectedDeliveryChanged)
                {
                    await UpdateRequestPOsAndHistory(poLogEntry, updateDto);
                }

                if (updateDto.UpdateAllDates)
                {
                    await UpdateAllPODeliveryDates(poLogEntry.Ponum!, updateDto.ExpectedDelivery, updateDto.UserId!);
                }

                // Add new notes if present
                if (!string.IsNullOrEmpty(updateDto.NewNote))
                {
                    await AddNewNoteAsync(poLogEntry, updateDto.NewNote, updateDto.UserName);
                }

                // Send Email if conditions are met
                if (expectedDeliveryChanged)
                {
                    await CheckAndSendDeliveryDateEmail(poLogEntry, updateDto);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PO detail for ID {Id}", id);
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task CheckAndSendDeliveryDateEmail(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            _logger.LogInformation("Checking delivery date email conditions for Sales Order {Sonum}", poLogEntry.SalesOrderNum);
            try
            {
                var salesOrder = await _context.QtSalesOrders.FirstOrDefaultAsync(s => s.RwsalesOrderNum == poLogEntry.SalesOrderNum);
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

                        var salesRep = await _context.Users.FirstOrDefaultAsync(u => u.Id == salesOrder.AccountMgr);
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
                                    ? $"*** PO#{updateDto.PONum} DELAYED BY {(updateDto.ExpectedDelivery.Value - saleReqDate.Value).Days} DAYS ***"
                                    : $"PO#{updateDto.PONum} Delivery Date Exceeds Required Date for SO#{poLogEntry.SalesOrderNum}"
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

                var email = new EmailInput
                {
                    FromEmail = null, // Update sendAs permissions if needed
                    ToEmail = emailInput.ToEmail,
                    Subject = emailInput.Subject,
                    HtmlBody = emailBody,
                    UserName = updateDto.UserName,
                    Password = updateDto.Password,
                    CCEmails = [],
                    Attachments = [],
                    Urgent = emailInput.Urgent
                };

                await _emailService.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }

        private async Task AddNewNoteAsync(TrkPolog poLogEntry, string note, string enteredBy)
        {
            var newSoNote = new TrkSonote
            {
                OrderNo = poLogEntry.SalesOrderNum,
                PartNo = poLogEntry.ItemNum,
                Notes = note,
                EnteredBy = enteredBy,
                EntryDate = DateTime.Now,
                ModBy = enteredBy,
                ModDate = DateTime.Now
            };

            var newPoNote = new TrkPonote
            {
                Ponum = int.TryParse(poLogEntry.Ponum, out int parsedPonum) ? parsedPonum : null,
                EnteredBy = enteredBy,
                EntryDate = DateTime.Now,
                Notes = note
            };

            await _context.TrkSonotes.AddAsync(newSoNote);
            await _context.TrkPonotes.AddAsync(newPoNote);
            await InsertCAMActivityAsync(poLogEntry.ContactId ?? 0, poLogEntry.Ponum!, note, enteredBy);
        }

        private async Task<int?> GetContactIdIfMissing(string poNum)
        {
            return await _context.RequestPos
                .Where(rp => rp.Ponum == poNum)
                .OrderByDescending(rp => rp.ContactId)
                .Select(rp => rp.ContactId)
                .FirstOrDefaultAsync();
        }

        private static void UpdatePODetailFields(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            poLogEntry.EditDate = DateTime.Now;
            poLogEntry.EditedBy = updateDto.UserId!;
            poLogEntry.ContactId = updateDto.ContactID;
            poLogEntry.ExpectedDelivery = updateDto.ExpectedDelivery;

            if (poLogEntry.ExpectedDelivery != updateDto.ExpectedDelivery)
            {
                poLogEntry.ExpDelEditDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private async Task UpdateRequestPOsAndHistory(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            var requestPOs = await _context.RequestPos
                .Join(_context.EquipmentRequests,
                      po => po.RequestId,
                      req => req.RequestId,
                      (po, req) => new { po, req })
                .Where(joined => joined.po.Ponum == poLogEntry.Ponum && joined.req.PartNum == poLogEntry.ItemNum)
                .Select(joined => joined.po)
                .ToListAsync();

            foreach (var requestPO in requestPOs)
            {
                var history = new RequestPohistory
                {
                    Poid = requestPO.Id,
                    Ponum = requestPO.Ponum,
                    DeliveryDate = requestPO.DeliveryDate,
                    QtyBought = requestPO.QtyBought,
                    EnteredBy = updateDto.UserId,
                    EditDate = requestPO.EditDate
                };

                _context.RequestPohistories.Add(history);
                requestPO.DeliveryDate = updateDto.ExpectedDelivery;
                requestPO.EditedBy = updateDto.UserId;
                requestPO.EditDate = DateTime.Now;
                _context.RequestPos.Update(requestPO);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateAllPODeliveryDates(string poNum, DateTime? expectedDelivery, int userId)
        {
            var poLogs = await _context.TrkPologs.Where(p => p.Ponum == poNum).ToListAsync();
            foreach (var poLog in poLogs)
            {
                poLog.ExpectedDelivery = expectedDelivery;
                poLog.ExpDelEditDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                poLog.EditDate = DateTime.Now;
                poLog.EditedBy = userId;
            }

            await _context.SaveChangesAsync();
        }

        private async Task InsertCAMActivityAsync(int contactId, string poNum, string note, string enteredBy)
        {
            string camNote = $"OPEN PO UPDATE FOR PO#{poNum}: {note}";

            var camActivity = new CamActivity
            {
                ContactId = contactId,
                ActivityOwner = enteredBy,
                ActivityType = "CallOut",
                DurationHours = 0,
                DurationMins = 0,
                ProjectCode = "",
                Notes = camNote,
                ActivityDate = DateTime.Now,
                EnteredBy = enteredBy,
                ModifiedBy = enteredBy,
                CompletedBy = enteredBy,
                ModifiedDate = DateTime.Now,
                CompleteDate = DateTime.Now,
                ContactOverride = 0,
            };

            await _context.CamActivities.AddAsync(camActivity);
        }
    }
}

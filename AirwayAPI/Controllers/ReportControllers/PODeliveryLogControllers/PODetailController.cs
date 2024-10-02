using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODetailController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<PODetailController> _logger;

        public PODetailController(
            eHelpDeskContext context, EmailService emailService, ILogger<PODetailController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // PUT: api/PODetail/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePODetail(int id, [FromBody] PODetailUpdateDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var poLogEntry = await _context.TrkPologs.FindAsync(id);
            if (poLogEntry == null)
            {
                return NotFound("PO log entry not found.");
            }

            // If ContactID is not provided, retrieve from RequestPOs
            if (updateDto.ContactID == 0)
            {
                var contactId = await GetContactIdIfMissing(updateDto.PONum);
                if (contactId.HasValue)
                {
                    updateDto.ContactID = contactId.Value;
                }
                else
                {
                    return BadRequest("Contact ID is missing and could not be found in RequestPOs.");
                }
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

                if (expectedDeliveryChanged)
                {
                    await UpdateRequestPOsAndHistory(poLogEntry, updateDto);
                }

                if (updateDto.UpdateAllDates)
                {
                    await UpdateAllPODeliveryDates(poLogEntry.Ponum!, updateDto.ExpectedDelivery, updateDto.UserId!.Value);
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Notes))
                {
                    var newNote = new TrkSonote
                    {
                        OrderNo = poLogEntry.SalesOrderNum,
                        PartNo = poLogEntry.ItemNum,
                        Notes = updateDto.Notes,
                        EnteredBy = updateDto.UserName,
                        EntryDate = DateTime.Now,
                        ModBy = updateDto.UserName,
                        ModDate = DateTime.Now
                    };

                    // Add the note to the database
                    _context.TrkSonotes.Add(newNote);

                    // Insert CAM Activity related to the note
                    InsertCAMActivity(updateDto.ContactID, poLogEntry.Ponum!, poLogEntry.ItemNum!, updateDto);

                    // Save changes to the context
                    await _context.SaveChangesAsync();
                }

                // Send Email if expected delivery date changes and exceeds required date
                if (expectedDeliveryChanged)
                {
                    await _emailService.CheckAndSendDeliveryDateEmail(poLogEntry, updateDto);
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

        private async Task<int?> GetContactIdIfMissing(string poNum)
        {
            var contactId = await (from rp in _context.RequestPos
                                   where rp.Ponum == poNum
                                   orderby rp.ContactId descending
                                   select rp.ContactId).FirstOrDefaultAsync();
            return contactId;
        }

        private void UpdatePODetailFields(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            poLogEntry.EditDate = DateTime.Now;
            poLogEntry.EditedBy = updateDto.UserId!.Value;
            poLogEntry.ContactId = updateDto.ContactID;
            poLogEntry.ExpectedDelivery = updateDto.ExpectedDelivery;

            if (poLogEntry.ExpectedDelivery != updateDto.ExpectedDelivery)
            {
                poLogEntry.ExpDelEditDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private async Task UpdateRequestPOsAndHistory(TrkPolog poLogEntry, PODetailUpdateDto updateDto)
        {
            var requestPOs = await (from re in _context.RequestEvents
                                    join er in _context.EquipmentRequests on re.EventId equals er.EventId
                                    join rp in _context.RequestPos on er.RequestId equals rp.RequestId
                                    where rp.Ponum == poLogEntry.Ponum && er.PartNum == poLogEntry.ItemNum
                                    select rp)
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

                // Update the RequestPO record
                requestPO.DeliveryDate = updateDto.ExpectedDelivery;
                requestPO.EditedBy = updateDto.UserId;
                requestPO.EditDate = DateTime.Now;

                _context.RequestPos.Update(requestPO);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateAllPODeliveryDates(string poNum, DateTime? expectedDelivery, int userId)
        {
            try
            {
                var poLogs = await _context.TrkPologs
                    .Where(p => p.Ponum == poNum)
                    .ToListAsync();

                foreach (var poLog in poLogs)
                {
                    poLog.ExpectedDelivery = expectedDelivery;
                    poLog.ExpDelEditDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    poLog.EditDate = DateTime.Now;
                    poLog.EditedBy = userId;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating all PO delivery dates.", ex);
            }
        }

        private void InsertCAMActivity(int contactId, string poNum, string itemNum, PODetailUpdateDto updateDto)
        {
            try
            {
                string camNote = $"OPEN PO UPDATE FOR PO# {poNum}\n\n";

                if (updateDto.ExpectedDelivery.HasValue)
                {
                    camNote += $"New Del Date: {updateDto.ExpectedDelivery.Value.ToShortDateString()}\n\n";
                }

                // Add notes
                camNote += $"Note: {updateDto.Notes}";

                var camActivity = new CamActivity
                {
                    ContactId = contactId,
                    ActivityOwner = updateDto.UserName,
                    ActivityType = "CallOut",
                    ProjectCode = "",
                    Notes = camNote,
                    ActivityDate = DateTime.Now,
                    EnteredBy = updateDto.UserName,
                    ModifiedBy = updateDto.UserName,
                    CompletedBy = updateDto.UserName,
                    CompleteDate = DateTime.Now
                };
                _context.CamActivities.Add(camActivity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error inserting CAM activity.", ex);
            }
        }
    }
}

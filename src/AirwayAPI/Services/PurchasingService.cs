using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.GenericDtos;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services;

public class PurchasingService(eHelpDeskContext context, IEmailService emailService, ILogger<PurchasingService> logger) : IPurchasingService
{
    private readonly eHelpDeskContext _context = context;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<PurchasingService> _logger = logger;

    public async Task<List<PODeliveryLogSearchResult>> GetPODeliveryLogsAsync(PODeliveryLogQueryParameters p)
    {
        if (p.YearRange == 0) p.YearRange = DateTime.Now.Year;
        var dateStart = new DateTime(p.YearRange, 1, 1);
        var dateEnd = new DateTime(p.YearRange, 12, 31);
        var companyAll = string.Equals(p.CompanyID, "all", StringComparison.OrdinalIgnoreCase);
        var companyParam = p.CompanyID ?? "";

        _logger.LogInformation(
            "Retrieving PODeliveryLogs for Year={Year}, CompanyID={CompanyID}",
            p.YearRange, p.CompanyID
        );

        // Base query, with null‑safe company filter
        var query = _context.TrkPologs
            .AsNoTracking()
            .Where(log =>
                log.Deleted != true &&
                log.IssueDate >= dateStart &&
                log.IssueDate <= dateEnd &&
                (companyAll || (log.CompanyId ?? "") == companyParam)
            )
            .Select(log => new PODeliveryLogSearchResult
            {
                Id = log.Id,
                Ponum = log.Ponum ?? "",
                IssueDate = log.IssueDate,
                ItemNum = log.ItemNum ?? "",
                QtyOrdered = log.QtyOrdered,
                QtyReceived = log.QtyReceived,
                ReceiverNum = log.ReceiverNum,
                NotesExist = _context.TrkPonotes.Any(n => n.Ponum.ToString() == log.Ponum),
                NoteEditDate = _context.TrkPonotes
                                      .Where(n => n.Ponum.ToString() == log.Ponum)
                                      .OrderByDescending(n => n.EntryDate)
                                      .Select(n => n.EntryDate.HasValue
                                          ? n.EntryDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                          : null)
                                      .FirstOrDefault(),
                PORequiredDate = log.RequiredDate,
                DateDelivered = log.DateDelivered,
                EditDate = log.EditDate,
                ExpectedDelivery = log.ExpectedDelivery,
                Sonum = log.SalesOrderNum ?? "",
                IssuedBy = log.IssuedBy ?? "",
                VendorName = _context.TrkRwPoheaders
                                        .Where(h => h.Ponum == log.Ponum)
                                        .OrderByDescending(h => h.Ponum)
                                        .Select(h => h.VendorNum)
                                        .Distinct()
                                        .Join(_context.TrkRwvendors,
                                              vn => vn,
                                              v => v.VendorNum,
                                              (_, v) => v.VendorName)
                                        .FirstOrDefault() ?? "",
                ItemClassId = _context.TrkRwImItems
                                        .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                                        .OrderByDescending(i => i.ItemNum)
                                        .Select(i => i.ItemClassId)
                                        .FirstOrDefault(),
                AltPartNum = _context.TrkRwImItems
                                        .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                                        .OrderByDescending(i => i.ItemNum)
                                        .Select(i => i.AltPartNum)
                                        .FirstOrDefault() ?? "",
                Postatus = _context.TrkRwPoheaders
                                        .Where(h => h.Ponum == log.Ponum)
                                        .OrderByDescending(h => h.Ponum)
                                        .Select(h => h.Postatus)
                                        .FirstOrDefault(),
                CompanyId = log.CompanyId ?? "",
                ContactId = log.ContactId,
                SalesRep = log.SalesRep ?? "",
                CustomerName = _context.TrkRwSoheaders
                                        .Where(s => s.OrderNum == log.SalesOrderNum)
                                        .OrderByDescending(s => s.OrderNum)
                                        .Select(s => s.CustomerName)
                                        .FirstOrDefault() ?? "",
                SORequiredDate = _context.TrkRwSoheaders
                                        .Where(s => s.OrderNum == log.SalesOrderNum)
                                        .OrderByDescending(s => s.OrderNum)
                                        .Select(s => s.RequiredDate)
                                        .FirstOrDefault(),
                IsDropShipment = _context.QtSalesOrders
                                        .Where(q => q.RwsalesOrderNum == log.SalesOrderNum)
                                        .OrderByDescending(q => q.SaleId)
                                        .Select(q => q.DropShipment)
                                        .FirstOrDefault() == true
            });

        // apply optional filters
        if (!string.IsNullOrEmpty(p.PONum))
            query = query.Where(r => r.Ponum.Contains(p.PONum));
        if (!string.IsNullOrEmpty(p.SONum))
            query = query.Where(r => r.Sonum.Contains(p.SONum));
        if (!string.IsNullOrEmpty(p.Vendor))
            query = query.Where(r => r.VendorName.Contains(p.Vendor));
        if (!string.IsNullOrEmpty(p.PartNum))
            query = query.Where(r => r.ItemNum.Contains(p.PartNum));
        if (!string.IsNullOrEmpty(p.xSalesRep)
            && !string.Equals(p.xSalesRep, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.SalesRep == p.xSalesRep);
        }

        // POStatus
        switch (p.POStatus.ToLowerInvariant())
        {
            case "not complete":
                query = query.Where(r => r.QtyOrdered > r.QtyReceived && r.Postatus == 1);
                break;
            case "complete":
                query = query.Where(r => r.QtyOrdered <= r.QtyReceived);
                break;
            case "late":
                query = query.Where(r =>
                    r.QtyOrdered > r.QtyReceived &&
                    r.Postatus == 1 &&
                    (
                        (r.ExpectedDelivery.HasValue && DateTime.Now > r.ExpectedDelivery.Value && r.PORequiredDate.HasValue && DateTime.Now > r.PORequiredDate.Value) ||
                        (r.PORequiredDate.HasValue && DateTime.Now > r.PORequiredDate.Value && !r.ExpectedDelivery.HasValue)
                    )
                );
                break;
            case "due w/n 2 days":
                query = query.Where(r =>
                    r.QtyOrdered > r.QtyReceived &&
                    r.Postatus == 1 &&
                    (
                        (r.ExpectedDelivery.HasValue && r.ExpectedDelivery.Value >= DateTime.Now && r.ExpectedDelivery.Value <= DateTime.Now.AddDays(2)) ||
                        (r.PORequiredDate.HasValue && r.PORequiredDate.Value >= DateTime.Now && r.PORequiredDate.Value <= DateTime.Now.AddDays(2) && !r.ExpectedDelivery.HasValue)
                    )
                );
                break;
        }

        // IssuedBy
        if (!string.IsNullOrEmpty(p.IssuedBy)
            && !string.Equals(p.IssuedBy, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.IssuedBy == p.IssuedBy);
        }

        // HasNotes
        if (string.Equals(p.HasNotes, "yes", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => r.NotesExist);
        else if (string.Equals(p.HasNotes, "no", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => !r.NotesExist);

        // EquipType
        if (string.Equals(p.EquipType, "anc", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => r.ItemClassId == 23);
        else if (string.Equals(p.EquipType, "fne", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => r.ItemClassId == 24);

        var results = await query.ToListAsync();
        _logger.LogInformation("Retrieved {Count} PODeliveryLogs after filtering", results.Count);
        return results;
    }

    public async Task<List<string>> GetVendorsAsync(PODeliveryLogQueryParameters p)
    {
        if (p.YearRange == 0) p.YearRange = DateTime.Now.Year;
        var dateStart = new DateTime(p.YearRange, 1, 1);
        var dateEnd = new DateTime(p.YearRange, 12, 31);
        var filterCompany = !string.IsNullOrEmpty(p.CompanyID)
                          && !string.Equals(p.CompanyID, "all", StringComparison.OrdinalIgnoreCase);
        var companyParam = p.CompanyID ?? "";

        _logger.LogInformation(
            "Retrieving Vendors for Year={Year}, CompanyID={CompanyID}",
            p.YearRange, p.CompanyID
        );

        // Join Pologs → Poheaders → Rwvendors
        var vendorsQuery = _context.TrkPologs
            .AsNoTracking()
            .Where(log =>
                log.Deleted != true &&
                log.IssueDate >= dateStart &&
                log.IssueDate <= dateEnd &&
                (!filterCompany || (log.CompanyId ?? "") == companyParam)
            )
            .Join(_context.TrkRwPoheaders,
                  log => log.Ponum,
                  hdr => hdr.Ponum,
                  (log, hdr) => new { log, hdr })
            .Join(_context.TrkRwvendors,
                  combined => combined.hdr.VendorNum,
                  v => v.VendorNum,
                  (combined, v) => new
                  {
                      combined.log.Ponum,
                      combined.log.SalesOrderNum,
                      combined.log.ItemNum,
                      combined.log.SalesRep,
                      combined.log.IssuedBy,
                      combined.log.ExpectedDelivery,
                      combined.hdr.RequiredDate,
                      combined.log.QtyOrdered,
                      combined.log.QtyReceived,
                      combined.hdr.Postatus,
                      VendorName = v.VendorName ?? "",
                      HasNote = _context.TrkPonotes.Any(n => n.Ponum.ToString() == combined.log.Ponum)
                  });

        // Optional filters
        if (!string.IsNullOrEmpty(p.PONum))
            vendorsQuery = vendorsQuery.Where(x => x.Ponum.Contains(p.PONum));
        if (!string.IsNullOrEmpty(p.SONum))
            vendorsQuery = vendorsQuery.Where(x => x.SalesOrderNum.Contains(p.SONum));
        if (!string.IsNullOrEmpty(p.PartNum))
            vendorsQuery = vendorsQuery.Where(x => x.ItemNum.Contains(p.PartNum));
        if (!string.IsNullOrEmpty(p.xSalesRep)
            && !string.Equals(p.xSalesRep, "all", StringComparison.OrdinalIgnoreCase))
        {
            vendorsQuery = vendorsQuery.Where(x => x.SalesRep == p.xSalesRep);
        }
        if (!string.IsNullOrEmpty(p.IssuedBy)
            && !string.Equals(p.IssuedBy, "all", StringComparison.OrdinalIgnoreCase))
        {
            vendorsQuery = vendorsQuery.Where(x => x.IssuedBy == p.IssuedBy);
        }

        // HasNotes
        if (string.Equals(p.HasNotes, "yes", StringComparison.OrdinalIgnoreCase))
            vendorsQuery = vendorsQuery.Where(x => x.HasNote);
        else if (string.Equals(p.HasNotes, "no", StringComparison.OrdinalIgnoreCase))
            vendorsQuery = vendorsQuery.Where(x => !x.HasNote);

        // POStatus
        switch (p.POStatus.ToLowerInvariant())
        {
            case "not complete":
                vendorsQuery = vendorsQuery.Where(x => x.QtyOrdered > x.QtyReceived && x.Postatus == 1);
                break;
            case "complete":
                vendorsQuery = vendorsQuery.Where(x => x.QtyOrdered <= x.QtyReceived);
                break;
            case "late":
                vendorsQuery = vendorsQuery.Where(x =>
                    x.QtyOrdered > x.QtyReceived &&
                    x.Postatus == 1 &&
                    (
                        (x.ExpectedDelivery.HasValue && DateTime.Now > x.ExpectedDelivery.Value && x.RequiredDate.HasValue && DateTime.Now > x.RequiredDate.Value) ||
                        (x.RequiredDate.HasValue && DateTime.Now > x.RequiredDate.Value && !x.ExpectedDelivery.HasValue)
                    )
                );
                break;
            case "due w/n 2 days":
                vendorsQuery = vendorsQuery.Where(x =>
                    x.QtyOrdered > x.QtyReceived &&
                    x.Postatus == 1 &&
                    (
                        (x.ExpectedDelivery.HasValue && x.ExpectedDelivery.Value >= DateTime.Now && x.ExpectedDelivery.Value <= DateTime.Now.AddDays(2)) ||
                        (x.RequiredDate.HasValue && x.RequiredDate.Value >= DateTime.Now && x.RequiredDate.Value <= DateTime.Now.AddDays(2) && !x.ExpectedDelivery.HasValue)
                    )
                );
                break;
        }

        // EquipType
        if (string.Equals(p.EquipType, "anc", StringComparison.OrdinalIgnoreCase))
            vendorsQuery = vendorsQuery.Where(x => x.VendorName == "anc");
        else if (string.Equals(p.EquipType, "fne", StringComparison.OrdinalIgnoreCase))
            vendorsQuery = vendorsQuery.Where(x => x.VendorName == "fne");

        var vendors = await vendorsQuery
            .Select(x => x.VendorName)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} Vendors after filtering", vendors.Count);
        return vendors;
    }

    public async Task<PODetailUpdateDto> GetPODetailByIdAsync(int id)
    {
        var raw = await _context.TrkPologs
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
                EditedBy = _context.Users
                                .Where(u => u.Id == p.EditedBy)
                                .Select(u => u.Uname)
                                .FirstOrDefault(),
                p.ExpDelEditDate,
                Notes = _context.TrkPonotes
                          .Where(n => n.Ponum.ToString() == p.Ponum)
                          .OrderByDescending(n => n.EntryDate)
                          .Select(n => new { n.Notes, n.EntryDate, n.EnteredBy })
                          .ToList(),
                Contact = _context.CamContacts
                            .Where(c => c.Id == p.ContactId)
                            .Select(c => new
                            {
                                c.Contact,
                                c.Title,
                                c.Company,
                                Phone = !string.IsNullOrEmpty(c.PhoneDirect)
                                            ? c.PhoneDirect
                                            : c.PhoneMain
                            })
                            .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (raw == null)
            throw new KeyNotFoundException($"PO detail with ID {id} not found.");

        // Fallback: if ContactId missing, try RequestPos
        int? contactId = raw.ContactId;
        if (!contactId.HasValue && !string.IsNullOrEmpty(raw.Ponum))
        {
            contactId = await GetContactIdIfMissing(raw.Ponum);
        }

        var contactDetails = contactId.HasValue
            ? await _context.CamContacts
                .Where(c => c.Id == contactId)
                .Select(c => new
                {
                    c.Contact,
                    c.Title,
                    c.Company,
                    Phone = !string.IsNullOrEmpty(c.PhoneDirect)
                                ? c.PhoneDirect
                                : c.PhoneMain
                })
                .FirstOrDefaultAsync()
            : null;

        var dto = new PODetailUpdateDto
        {
            Id = raw.Id,
            PONum = raw.Ponum ?? "",
            SONum = raw.SalesOrderNum ?? "",
            PartNum = raw.ItemNum,
            QtyOrdered = raw.QtyOrdered,
            QtyReceived = raw.QtyReceived,
            ReceiverNum = raw.ReceiverNum,
            ExpectedDelivery = raw.ExpectedDelivery,
            ContactID = contactId,
            IssuedBy = raw.IssuedBy ?? "",
            DateDelivered = raw.DateDelivered,
            EditDate = raw.EditDate,
            EditedBy = raw.EditedBy ?? "",
            ExpDelEditDate = raw.ExpDelEditDate ?? "",
            NotesList = raw.Notes
                .Select(n => $"{n.EnteredBy}::{n.Notes}::{(n.EntryDate?.ToShortDateString() ?? "No Date")}")
                .ToList(),
            ContactName = contactDetails?.Contact ?? "",
            Company = contactDetails?.Company ?? "",
            Title = contactDetails?.Title ?? "",
            Phone = contactDetails?.Phone ?? ""
        };

        return dto;
    }

    public async Task UpdatePODetailAsync(int id, PODetailUpdateDto updateDto)
    {
        if (id != updateDto.Id)
            throw new ArgumentException("ID mismatch between route and payload.");

        var poLogEntry = await _context.TrkPologs.FindAsync(id);
        if (poLogEntry == null)
            throw new KeyNotFoundException($"PO log entry {id} not found.");

        // Retrieve SONum if missing
        if (string.IsNullOrEmpty(updateDto.SONum))
        {
            var fallbackSo = await _context.EquipmentRequests
                .Where(r => r.PartNum == poLogEntry.ItemNum)
                .Join(_context.RequestPos,
                      r => r.RequestId,
                      p => p.RequestId,
                      (r, p) => new { p.Ponum, r.SalesOrderNum })
                .Where(x => x.Ponum == updateDto.PONum)
                .Select(x => x.SalesOrderNum)
                .FirstOrDefaultAsync();
            updateDto.SONum = fallbackSo ?? updateDto.SONum;
        }

        if (!updateDto.ExpectedDelivery.HasValue)
            throw new ArgumentException("Expected Delivery date is invalid or missing.");

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            bool dateChanged = poLogEntry.ExpectedDelivery != updateDto.ExpectedDelivery;

            // Core field updates
            UpdatePODetailFields(poLogEntry, updateDto);
            poLogEntry.QtyOrdered = updateDto.QtyOrdered;
            poLogEntry.QtyReceived = updateDto.QtyReceived;
            poLogEntry.ReceiverNum = updateDto.ReceiverNum;
            poLogEntry.ContactId = updateDto.ContactID;

            if (dateChanged)
                await UpdateRequestPOsAndHistory(poLogEntry, updateDto);

            if (updateDto.UpdateAllDates)
                await UpdateAllPODeliveryDates(poLogEntry.Ponum!, updateDto.ExpectedDelivery.Value, updateDto.UserId);

            if (dateChanged)
                await CheckAndSendDeliveryDateEmail(poLogEntry, updateDto);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task AddNoteAsync(int id, NoteDto noteDto)
    {
        var poLogEntry = await _context.TrkPologs.FindAsync(id) ?? throw new KeyNotFoundException($"PO log entry {id} not found.");

        // Add sales‐order note
        var soNote = new TrkSonote
        {
            OrderNo = poLogEntry.SalesOrderNum,
            PartNo = poLogEntry.ItemNum,
            EnteredBy = noteDto.EnteredBy,
            EntryDate = DateTime.Now,
            ModBy = noteDto.EnteredBy,
            ModDate = DateTime.Now,
            NoteType = "Note",
            Notes = noteDto.Note,
            ContactId = poLogEntry.ContactId
        };
        await _context.TrkSonotes.AddAsync(soNote);

        // Add PO note
        var poNumInt = int.TryParse(poLogEntry.Ponum, out var pn) ? (int?)pn : null;
        var poNote = new TrkPonote
        {
            Ponum = poNumInt,
            EnteredBy = noteDto.EnteredBy,
            EntryDate = DateTime.Now,
            Notes = noteDto.Note
        };
        await _context.TrkPonotes.AddAsync(poNote);

        // CAM activity
        await InsertCAMActivityAsync(poLogEntry.ContactId, poLogEntry.Ponum!, noteDto.Note, noteDto.EnteredBy);

        await _context.SaveChangesAsync();
    }

    // ─── private helpers ───────────────────────────────────────────────────────

    private static void UpdatePODetailFields(TrkPolog po, PODetailUpdateDto dto)
    {
        po.EditDate = DateTime.Now;
        po.EditedBy = dto.UserId;
        po.ExpectedDelivery = dto.ExpectedDelivery.Value;
        po.ExpDelEditDate = dto.ExpectedDelivery.Value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private async Task UpdateRequestPOsAndHistory(TrkPolog po, PODetailUpdateDto dto)
    {
        var requestPOs = await _context.RequestPos
            .Join(_context.EquipmentRequests,
                  poRec => poRec.RequestId,
                  req => req.RequestId,
                  (poRec, req) => new { poRec, req })
            .Where(x => x.poRec.Ponum == po.Ponum && x.req.PartNum == po.ItemNum)
            .Select(x => x.poRec)
            .ToListAsync();

        foreach (var rpo in requestPOs)
        {
            _context.RequestPohistories.Add(new RequestPohistory
            {
                Poid = rpo.Id,
                Ponum = rpo.Ponum,
                DeliveryDate = rpo.DeliveryDate,
                QtyBought = rpo.QtyBought,
                EnteredBy = dto.UserId,
                EditDate = rpo.EditDate
            });

            rpo.DeliveryDate = dto.ExpectedDelivery.Value;
            rpo.EditedBy = dto.UserId;
            rpo.EditDate = DateTime.Now;
            _context.RequestPos.Update(rpo);
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpdateAllPODeliveryDates(string poNum, DateTime newDate, int userId)
    {
        var logs = await _context.TrkPologs.Where(p => p.Ponum == poNum).ToListAsync();
        foreach (var log in logs)
        {
            log.ExpectedDelivery = newDate;
            log.ExpDelEditDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            log.EditDate = DateTime.Now;
            log.EditedBy = userId;
        }
        await _context.SaveChangesAsync();
    }

    private async Task CheckAndSendDeliveryDateEmail(TrkPolog po, PODetailUpdateDto dto)
    {
        _logger.LogInformation("Checking delivery‐date email for SO {So}", po.SalesOrderNum);

        var salesOrder = await _context.QtSalesOrders
            .FirstOrDefaultAsync(s => s.RwsalesOrderNum == po.SalesOrderNum);
        if (salesOrder?.RequiredDate == null || !dto.ExpectedDelivery.HasValue)
            return;

        if (dto.ExpectedDelivery.Value > salesOrder.RequiredDate.Value && po.DeliveryDateEmail != true)
        {
            po.DeliveryDateEmail = true;

            var rep = await _context.Users.FirstOrDefaultAsync(u => u.Id == salesOrder.AccountMgr);
            if (rep?.Email != null)
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["{{SoNum}}"] = po.SalesOrderNum ?? "",
                    ["{{SalesRep}}"] = rep.Uname,
                    ["{{CompanyName}}"] = salesOrder.ShipToCompanyName ?? "",
                    ["{{SalesRequiredDate}}"] = salesOrder.RequiredDate.Value.ToShortDateString(),
                    ["{{ExpectedDeliveryDate}}"] = dto.ExpectedDelivery.Value.ToShortDateString(),
                    ["{{PartNumber}}"] = po.ItemNum,
                    ["{{Notes}}"] = dto.NewNote ?? ""
                };

                var emailInput = new PODetailEmailInput
                {
                    ToEmails = [rep.Email],
                    FromEmail = "purch_dept@airway.com",
                    UserName = dto.UserName,
                    Password = dto.Password,
                    Subject = dto.UrgentEmail
                                     ? $"*** PO#{dto.PONum} DELAYED ***"
                                     : $"PO#{dto.PONum} Delayed",
                    Placeholders = placeholders,
                    SoNum = po.SalesOrderNum,
                    SalesRep = rep.Uname,
                    CompanyName = salesOrder.ShipToCompanyName,
                    SalesRequiredDate = salesOrder.RequiredDate.Value.ToShortDateString(),
                    ExpectedDeliveryDate = dto.ExpectedDelivery.Value.ToShortDateString(),
                    PartNumber = po.ItemNum,
                    Notes = dto.NewNote
                };

                await _emailService.SendEmailAsync(emailInput);
            }
            else
            {
                _logger.LogWarning("No email for rep on SO {So}", po.SalesOrderNum);
            }
        }
    }

    private async Task InsertCAMActivityAsync(int? contactId, string poNum, string note, string enteredBy)
    {
        if (!contactId.HasValue) return;

        var cam = new CamActivity
        {
            ContactId = contactId.Value,
            ActivityOwner = enteredBy,
            ActivityType = "CallOut",
            DurationHours = 0,
            DurationMins = 0,
            ProjectCode = "",
            Notes = $"OPEN PO UPDATE FOR PO#{poNum}: {note}",
            ActivityDate = DateTime.Now,
            EnteredBy = enteredBy,
            ModifiedBy = enteredBy,
            CompletedBy = enteredBy,
            ModifiedDate = DateTime.Now,
            CompleteDate = DateTime.Now,
            ContactOverride = 0
        };
        await _context.CamActivities.AddAsync(cam);
    }

    private async Task<int?> GetContactIdIfMissing(string poNum)
    {
        return await _context.RequestPos
            .Where(rp => rp.Ponum == poNum && rp.ContactId.HasValue)
            .OrderByDescending(rp => rp.EntryDate)
            .Select(rp => rp.ContactId)
            .FirstOrDefaultAsync();
    }
}
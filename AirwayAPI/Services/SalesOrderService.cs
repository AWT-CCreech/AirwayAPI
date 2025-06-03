using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Models.SalesOrderWorkbenchModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services;

public class SalesOrderService(
    eHelpDeskContext context,
    ILogger<SalesOrderService> logger,
    IEmailService emailService,
    IUserService userService,
    IStringService stringService
) : ISalesOrderService
{
    private readonly eHelpDeskContext _context = context;
    private readonly ILogger<SalesOrderService> _logger = logger;
    private readonly IEmailService _emailService = emailService;
    private readonly IUserService _userService = userService;
    private readonly IStringService _stringService = stringService;

    #region 1) GET: (Event-Level & Detail-Level)

    public async Task<List<object>> GetEventLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId)
    {
        // Matches "RWSalesOrderNum = '0' And Draft=0" from old ASP
        var query = from so in _context.QtSalesOrders
                    join u in _context.Users on so.AccountMgr equals u.Id into userJoin
                    from usr in userJoin.DefaultIfEmpty()
                    where so.RwsalesOrderNum == "0" && so.Draft == false
                    select new
                    {
                        so.SaleId,
                        so.EventId,
                        so.QuoteId,
                        so.Version,
                        so.BillToCompanyName,
                        so.SaleTotal,
                        so.SaleDate,
                        so.AccountMgr,
                        SalesRep = usr != null ? usr.Uname : "N/A",
                        so.DropShipment
                    };

        // Apply filters
        if (eventId.HasValue && eventId.Value != 0)
            query = query.Where(q => q.EventId == eventId.Value);
        if (salesRepId.HasValue && salesRepId.Value != 0)
            query = query.Where(q => q.AccountMgr == salesRepId.Value);
        if (!string.IsNullOrWhiteSpace(billToCompany))
            query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));

        var sql = query.ToQueryString();
        _logger.LogDebug("GetEventLevelData SQL Query: {sql}", sql);

        var result = await query.OrderBy(q => q.EventId).ToListAsync<object>();
        return result;
    }

    public async Task<List<object>> GetDetailLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId)
    {
        // Matches the old ASP "SELECT d.* from qtSalesOrderDetail d
        //  INNER JOIN qtSalesOrders so ON d.SaleId=so.SaleId
        //  WHERE d.SOFlag=1 AND so.RWSalesOrderNum='0'..."
        // But you can adjust if you want RWSalesOrderNum=some value.
        var query = from d in _context.QtSalesOrderDetails
                    join so in _context.QtSalesOrders on d.SaleId equals so.SaleId
                    join er in _context.EquipmentRequests on d.RequestId equals er.RequestId
                    join u in _context.Users on so.AccountMgr equals u.Id into userJoin
                    from usr in userJoin.DefaultIfEmpty()
                    where d.Soflag == true
                    select new
                    {
                        d.Id,
                        d.RequestId,
                        d.QtySold,
                        d.UnitMeasure,
                        d.PartNum,
                        d.PartDesc,
                        d.UnitPrice,
                        d.ExtendedPrice,
                        er.SalesOrderNum,
                        so.RwsalesOrderNum,
                        so.EventId,
                        so.BillToCompanyName,
                        so.AccountMgr,
                        SalesRep = usr != null ? usr.Uname : "N/A",
                        er.DropShipment
                    };

        // Filters
        if (eventId.HasValue && eventId.Value != 0)
            query = query.Where(q => q.EventId == eventId.Value);
        if (salesRepId.HasValue && salesRepId.Value != 0)
            query = query.Where(q => q.AccountMgr == salesRepId.Value);
        if (!string.IsNullOrWhiteSpace(billToCompany))
            query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));

        var sql = query.ToQueryString();
        _logger.LogDebug("GetDetailLevelData SQL Query: {sql}", sql);

        var result = await query.OrderBy(q => q.RequestId).ToListAsync<object>();
        return result;
    }
    #endregion

    #region 2) POST: (UpdateEventLevel & UpdateDetailLevel)

    /// <summary>
    /// Update the SalesOrder record and related entities at the event level,
    /// replicating the old ASP logic (Verizon vs Non-Verizon, dropping shipments, etc.).
    /// </summary>
    public async Task UpdateEventLevelAsync(EventLevelUpdateDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Load the QtSalesOrder
            var salesOrder = await _context.QtSalesOrders
                .FirstOrDefaultAsync(so => so.SaleId == request.SaleId)
                ?? throw new Exception($"Sales order with SaleID#{request.SaleId} not found.");

            // 2) Overwrite RwsalesOrderNum, DropShipment
            //    Replace semicolons if user typed multiple SO #s.
            salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
            salesOrder.DropShipment = request.DropShipment;
            salesOrder.EditDate = DateTime.Now;
            _context.QtSalesOrders.Update(salesOrder);
            await _context.SaveChangesAsync();

            // 3) Update the related quote, if any
            await UpdateQuoteAsync(request);

            // 4) Update all detail-level items for this SaleID => set the EquipmentRequest as “Sold,” etc.
            var details = await _context.QtSalesOrderDetails
                .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                .ToListAsync();

            foreach (var detail in details)
            {
                await ProcessEquipmentRequest(detail, request);
            }

            // 5) Mark any other items for the Event as LOST if QtySold=0
            if (!salesOrder.EventId.HasValue)
                throw new Exception("SalesOrder is missing an EventId");

            await MarkUnsoldItemsAsLostAsync(salesOrder.EventId.Value);

            // 6) Fire off the advanced emailing logic (Verizon vs Non-Verizon, drop ship prepay, etc.)
            await SendEventLevelEmailsAsync(request, salesOrder);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("UpdateEventLevelAsync Transaction rolled back: {Message}", ex.Message);
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Update the detail-level record and its EquipmentRequest, replicating old logic.
    /// </summary>
    public async Task UpdateDetailLevelAsync(DetailLevelUpdateDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Retrieve the SalesOrderDetail record
            var detail = await _context.QtSalesOrderDetails
                .FirstOrDefaultAsync(d => d.RequestId == request.RequestId)
                ?? throw new Exception($"SalesOrderDetail not found for RequestID={request.RequestId}.");

            // 2) Retrieve the parent QtSalesOrder
            var salesOrder = await _context.QtSalesOrders
                .FirstOrDefaultAsync(so => so.SaleId == detail.SaleId)
                ?? throw new Exception($"SalesOrder not found for SaleID={detail.SaleId}.");

            // Overwrite RwsalesOrderNum from user input (like RWSalesNum2 in classic ASP)
            salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
            salesOrder.EditDate = DateTime.Now;
            _context.QtSalesOrders.Update(salesOrder);
            await _context.SaveChangesAsync();

            // 3) Update the Quote record if needed
            var quote = await _context.QtQuotes
                .FirstOrDefaultAsync(q => q.EventId == salesOrder.EventId && q.QuoteId == salesOrder.QuoteId);
            if (quote != null)
            {
                quote.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
                await _context.SaveChangesAsync();
            }

            // 4) Update the EquipmentRequest record itself
            var equipmentRequest = await _context.EquipmentRequests
                .FirstOrDefaultAsync(er => er.RequestId == request.RequestId)
                ?? throw new Exception($"EquipmentRequest not found for RequestID={request.RequestId}.");

            // Overwrite or append the new MAS # 
            equipmentRequest.SalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);

            // Mark as Sold
            equipmentRequest.Status = "Sold";
            equipmentRequest.MarkedSoldDate = DateTime.Now;

            // e.g. old ASP style: xQtySold = existing.QtySold + detail.QtySold
            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);

            // Decide if we can mark as Bought
            bool canMarkAsBought = await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0);
            equipmentRequest.Bought = canMarkAsBought;

            if (canMarkAsBought)
            {
                _context.TrkSoldItemHistories.Add(new TrkSoldItemHistory
                {
                    RequestId = equipmentRequest.RequestId,
                    Username = request.Username,
                    PageName = "UpdateDetailLevelAsync",
                    DateMarkedBought = DateTime.Now
                });
            }

            equipmentRequest.ModifiedBy = await _userService.GetUserIdAsync(request.Username);
            equipmentRequest.ModifiedDate = DateTime.Now;

            _context.EquipmentRequests.Update(equipmentRequest);
            await _context.SaveChangesAsync();

            // 5) Optionally send email to replicate old detail-level email logic 
            await NotifyDetailLevelChanges(request, equipmentRequest);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("UpdateDetailLevelAsync Transaction rolled back: {Message}", ex.Message);
            await transaction.RollbackAsync();
            throw;
        }
    }

    #endregion

    #region 3) Internal Helper Methods (Email, Mark Lost, Bought, etc.)

    /// <summary>
    /// Process each EquipmentRequest for the event-level update:
    ///   - Overwrite/append the SalesOrderNum
    ///   - Mark Sold, record quantity, handle "Bought"
    ///   - Write to history
    /// </summary>
    private async Task ProcessEquipmentRequest(QtSalesOrderDetail detail, EventLevelUpdateDto request)
    {
        var eqReq = await _context.EquipmentRequests
            .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId)
            ?? throw new Exception($"EquipmentRequest with RequestID={detail.RequestId} not found.");

        // Overwrite or append the new MAS # if needed
        eqReq.SalesOrderNum = UpdateSalesOrderNum(eqReq.SalesOrderNum ?? "", request.SalesOrderNum);

        eqReq.Status = "Sold";
        eqReq.SalePrice = detail.UnitPrice;
        eqReq.MarkedSoldDate = DateTime.Now;

        eqReq.QtySold = (eqReq.QtySold ?? 0) + (detail.QtySold ?? 0);
        eqReq.DropShipment = request.DropShipment;
        eqReq.ModifiedBy = await _userService.GetUserIdAsync(request.Username);
        eqReq.ModifiedDate = DateTime.Now;

        bool canMarkAsBought = await ShouldMarkAsBoughtAsync(eqReq, detail.QtySold ?? 0);
        eqReq.Bought = canMarkAsBought;
        if (canMarkAsBought)
        {
            _context.TrkSoldItemHistories.Add(new TrkSoldItemHistory
            {
                RequestId = eqReq.RequestId,
                Username = request.Username,
                PageName = "SalesOrderWorkbenchService.ProcessEquipmentRequest",
                DateMarkedBought = DateTime.Now
            });
        }

        _context.EquipmentRequests.Update(eqReq);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Mark unsold items as LOST => "UPDATE EquipmentRequest SET Status='Lost' WHERE QtySold=0 AND EventID=..."
    /// </summary>
    private async Task MarkUnsoldItemsAsLostAsync(int eventId)
    {
        var items = await _context.EquipmentRequests
            .Where(er => er.EventId == eventId && (er.QtySold ?? 0) == 0)
            .ToListAsync();

        foreach (var item in items)
            item.Status = "Lost";

        if (items.Count != 0)
        {
            _context.EquipmentRequests.UpdateRange(items);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Marked {Count} items as LOST for EventID={EventId}", items.Count, eventId);
    }

    /// <summary>
    /// Decide if we can mark the item as Bought => replicate the "NeedToBuy <= 0" from ASP
    /// </summary>
    private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest request, int qtySoldThisLine)
    {
        // 1) Retrieve the relevant inventory data
        var inventory = await _context.TrkRwImItems
            .FirstOrDefaultAsync(i => i.ItemNum == request.PartNum || i.AltPartNum == request.PartNum);

        int QtyOnHand = inventory?.QtyOnHand ?? 0;
        int QtyInPick = inventory?.QtyInPick ?? 0;

        int Adjustments = await _context.TrkAdjustments
            .Where(a => a.ItemNum == request.PartNum && a.EntryDate == DateTime.Today)
            .SumAsync(a => a.QtyAdjustment) ?? 0;

        int QtyBought = await _context.RequestPos
            .Where(r => r.RequestId == request.RequestId)
            .SumAsync(r => r.QtyBought) ?? 0;

        int QtySoldToday = await _context.EquipmentRequests
            .Where(e => e.PartNum == request.PartNum
                        && e.Status == "Sold"
                        && e.MarkedSoldDate.HasValue
                        && e.MarkedSoldDate.Value.Date == DateTime.Today)
            .SumAsync(e => e.QtySold) ?? 0;

        // 2) Calculate how much is actually available to sell
        int QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;

        // 3) Compare: If (qtySoldThisLine - QtyAvailToSell - QtyBought) <= 0 => mark as Bought
        int difference = qtySoldThisLine - QtyAvailToSell - QtyBought;
        bool canMarkAsBought = difference <= 0;

        // 4) Log the breakdown either way
        if (canMarkAsBought)
        {
            _logger.LogInformation(
                "ShouldMarkAsBoughtAsync => RequestID {RequestId} IS marked as Bought. " +
                "QtySoldLine={QtySoldThisLine}, QtyOnHand={QtyOnHand}, QtyInPick={QtyInPick}, " +
                "Adjustments={Adjustments}, QtySoldToday={QtySoldToday}, QtyBought={QtyBought}, " +
                "QtyAvailToSell={QtyAvailToSell}, difference={Difference}",
                request.RequestId, qtySoldThisLine, QtyOnHand, QtyInPick, Adjustments,
                QtySoldToday, QtyBought, QtyAvailToSell, difference
            );
        }
        else
        {
            _logger.LogInformation(
                "ShouldMarkAsBoughtAsync => RequestID {RequestId} NOT marked as Bought. " +
                "Insufficient inventory or purchased qty to cover sold quantity. " +
                "QtySoldLine={QtySoldThisLine}, QtyOnHand={QtyOnHand}, QtyInPick={QtyInPick}, " +
                "Adjustments={Adjustments}, QtySoldToday={QtySoldToday}, QtyBought={QtyBought}, " +
                "QtyAvailToSell={QtyAvailToSell}, difference={Difference}",
                request.RequestId, qtySoldThisLine, QtyOnHand, QtyInPick, Adjustments,
                QtySoldToday, QtyBought, QtyAvailToSell, difference
            );
        }

        return canMarkAsBought;
    }

    /// <summary>
    /// Overwrite or append new SalesOrderNum. 
    /// In old ASP, if existing was non-empty, they'd sometimes do "existing, new".
    /// Just adjust as needed.
    /// </summary>
    private static string UpdateSalesOrderNum(string existing, string newNum)
    {
        // For pure overwrite: return newNum;
        // For append with commas:
        if (string.IsNullOrWhiteSpace(existing))
            return newNum;
        else
            return existing + ", " + newNum;
    }

    /// <summary>
    /// Update the QtQuote for the matching quote, setting RwsalesOrderNum
    /// </summary>
    private async Task UpdateQuoteAsync(EventLevelUpdateDto dto)
    {
        var quote = await _context.QtQuotes
            .FirstOrDefaultAsync(q => q.QuoteId == dto.QuoteId && q.EventId == dto.EventId);

        if (quote != null)
        {
            quote.RwsalesOrderNum = dto.SalesOrderNum.Replace(";", ",");
            await _context.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning("No related quote found for QuoteID={QuoteId}, EventID={EventId}",
                dto.QuoteId, dto.EventId);
        }
    }

    /// <summary>
    /// Send event-level emails replicating the old code:
    ///   - If BillTo=VERIZON => "A Verizon SO assigned..."
    ///   - Else check SO_Email_Notifications => "A <billto> SO assigned..."
    ///   - If soTerms="0" & drop=1 & PONum!="1234" => "PREPAYMENT ALERT" to multiple recipients
    /// </summary>
    private async Task SendEventLevelEmailsAsync(EventLevelUpdateDto request, QtSalesOrder salesOrder)
    {
        // 1) Check if BillTo starts with "VERIZON"
        if (salesOrder.BillToCompanyName != null &&
            salesOrder.BillToCompanyName.StartsWith("VERIZON", StringComparison.OrdinalIgnoreCase))
        {
            await _emailService.SendEmailAsync(new EmailInputBase
            {
                //FromEmail = "ITDept@airway.com",

                FromEmail = "ccreech@airway.com",
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"A Verizon SO has been assigned to Event ID {salesOrder.EventId}",
                Body = $"SO Number(s): {salesOrder.RwsalesOrderNum}",
                UserName = request.Username,
                Password = request.Password
            });
        }
        else
        {
            // 2) Non-Verizon => check if BillTo in SO_Email_Notifications
            bool inSoEmailNotify = await _context.SoEmailNotifications
                .AnyAsync(n => n.BillToCompanyName == salesOrder.BillToCompanyName);

            if (inSoEmailNotify)
            {
                await _emailService.SendEmailAsync(new EmailInputBase
                {
                    //FromEmail = "ITDept@airway.com",

                    FromEmail = "ccreech@airway.com",
                    ToEmails = new List<string> { "ccreech@airway.com" },
                    Subject = $"A {salesOrder.BillToCompanyName} SO has been assigned to Event ID {salesOrder.EventId}",
                    Body = $"SO Number(s): {salesOrder.RwsalesOrderNum}",
                    UserName = request.Username,
                    Password = request.Password
                });
            }

            // 3) Check if soTerms=0, DropShipment=1, PONum!="1234"
            //    => "PREPAYMENT ALERT" email
            // In old code, we joined RequestPOs or looked up s.Terms, p.PONum, etc.
            var soExtra = await (from s in _context.QtSalesOrders
                                 join e in _context.EquipmentRequests on s.EventId equals e.EventId
                                 join r in _context.RequestPos on e.RequestId equals r.RequestId into j
                                 from p in j.DefaultIfEmpty()
                                 where s.SaleId == salesOrder.SaleId
                                 select new
                                 {
                                     s.Terms,
                                     PONum = p.Ponum,
                                     PORep = p.PurchasedBy
                                 })
                                 .FirstOrDefaultAsync();

            if (soExtra != null)
            {
                string? soTerms = soExtra.Terms;
                string? poNum = soExtra.PONum ?? "";
                bool isDrop = salesOrder.DropShipment == true;
                bool isPrepayTerm = (soTerms == "0");
                bool notFakePO = (poNum != "1234");

                if (isDrop && isPrepayTerm && notFakePO)
                {
                    // The old code references dsSalesRepEmail, dsPurchEmail, "Acct_dept@airway.com", plus ccCreech
                    string dsSalesRepEmail = $"{request.Username}@airway.com";
                    string dsPurchEmail = "Purch_Dept@airway.com"; // or look up p.PORep?

                    // Send "PREPAYMENT ALERT" 
                    await _emailService.SendEmailAsync(new EmailInputBase
                    {
                        //FromEmail = "ITDept@airway.com",

                        FromEmail = "ccreech@airway.com",
                        ToEmails = new List<string> {
                            "ccreech@airway.com",
                            dsSalesRepEmail,
                            dsPurchEmail,
                            "Acct_dept@airway.com"
                        },
                        Subject = $"PREPAYMENT ALERT FOR SO {salesOrder.RwsalesOrderNum}",
                        Body = $@"
                                SO Number: {salesOrder.RwsalesOrderNum} has been flagged as a drop shipment. 
                                This is a reminder to collect the prepayment before the item(s) ship.
                                Sales Rep: {request.Username}
                                PO Number: {poNum}
                                (etc.)
                            ",
                        UserName = request.Username,
                        Password = request.Password
                    });
                }
            }
        }
    }

    private async Task NotifyDetailLevelChanges(DetailLevelUpdateDto request, EquipmentRequest eqReq)
    {
        // Advance logic if needed
        await _emailService.SendEmailAsync(new EmailInputBase
        {
            //FromEmail = "ITDept@airway.com",

            FromEmail = "ccreech@airway.com",
            ToEmails = new List<string> { "ccreech@airway.com" },
            Subject = $"Detail-level updated for RequestID={eqReq.RequestId}",
            Body = $"SO Number(s): {eqReq.SalesOrderNum}",
            UserName = request.Username,
            Password = request.Password
        });
    }

    #endregion
}

using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.SalesOrderWorkbenchModels;
using AirwayAPI.Models.ServiceModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers.SalesOrderWorkbenchControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<SalesOrderWorkbenchController> _logger;

        public SalesOrderWorkbenchController(
            eHelpDeskContext context,
            EmailService emailService,
            ILogger<SalesOrderWorkbenchController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/SalesOrderWorkbench/EventLevelData
        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData(
            int? salesRepId,
            string? billToCompany,
            int? eventId)
        {
            try
            {
                var query = from so in _context.QtSalesOrders
                            join mgr in _context.Users on so.AccountMgr equals mgr.Id into mgrJoin
                            from mgr in mgrJoin.DefaultIfEmpty()
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
                                SalesRep = mgr.Uname
                            };

                if (eventId.HasValue && eventId.Value != 0)
                {
                    query = query.Where(q => q.EventId == eventId.Value);
                }
                else
                {
                    if (salesRepId.HasValue && salesRepId.Value != 0)
                        query = query.Where(q => q.SaleId != null && q.SaleId == salesRepId.Value);

                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => !string.IsNullOrEmpty(q.BillToCompanyName) && EF.Functions.Like(q.BillToCompanyName, $"{billToCompany}%"));
                }

                var sqlQuery = query.ToQueryString();
                _logger.LogInformation("Executing SQL Query: {SqlQuery}", sqlQuery);

                var salesOrders = await query
                    .OrderBy(q => q.EventId)
                    .ToListAsync();

                return Ok(salesOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching EventLevelData: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching Event Level Data.");
            }
        }

        // GET: api/SalesOrderWorkbench/DetailLevelData
        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData(
            int? salesRepId,
            string? billToCompany,
            int? eventId)
        {
            try
            {
                var query = from detail in _context.QtSalesOrderDetails
                            join order in _context.QtSalesOrders on detail.SaleId equals order.SaleId
                            join mgr in _context.Users on order.AccountMgr equals mgr.Id into mgrJoin
                            from mgr in mgrJoin.DefaultIfEmpty()
                            where detail.Soflag == true
                            select new
                            {
                                detail.Id,
                                detail.RequestId,
                                detail.QtySold,
                                detail.UnitMeasure,
                                detail.PartNum,
                                detail.PartDesc,
                                detail.UnitPrice,
                                detail.ExtendedPrice,
                                SalesRep = mgr.Uname,
                                order.RwsalesOrderNum,
                                order.EventId,
                                order.AccountMgr,
                                order.BillToCompanyName
                            };

                if (eventId.HasValue && eventId.Value != 0)
                {
                    query = query.Where(q => q.EventId == eventId.Value);
                }
                else
                {
                    if (salesRepId.HasValue && salesRepId.Value != 0)
                        query = query.Where(q => q.AccountMgr == salesRepId.Value);

                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => !string.IsNullOrEmpty(q.BillToCompanyName) && EF.Functions.Like(q.BillToCompanyName, $"{billToCompany}%"));
                }

                var sqlQuery = query.ToQueryString();
                _logger.LogInformation("Executing SQL Query: {SqlQuery}", sqlQuery);

                var details = await query
                    .OrderBy(q => q.RequestId)
                    .ToListAsync();

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching DetailLevelData: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching Detail Level Data.");
            }
        }

        // POST: api/SalesOrderWorkbench/UpdateSalesOrder
        [HttpPost("UpdateSalesOrder")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update qtSalesOrder
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId);

                if (salesOrder == null)
                    return NotFound("Sales order not found.");

                salesOrder.RwsalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
                salesOrder.DropShipment = request.DropShipment;
                await _context.SaveChangesAsync();

                // Update qtQuote
                var quote = await _context.QtQuotes
                    .FirstOrDefaultAsync(q => q.EventId == request.EventId && q.QuoteId == request.QuoteId);
                if (quote != null)
                {
                    quote.RwsalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
                    await _context.SaveChangesAsync();
                }

                // Fetch SalesOrderDetails with QtySold > 0
                var salesOrderDetails = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .OrderBy(d => d.RequestId)
                    .ToListAsync();

                foreach (var detail in salesOrderDetails)
                {
                    // Fetch the corresponding EquipmentRequest
                    var equipmentRequest = await _context.EquipmentRequests
                        .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId);

                    if (equipmentRequest != null)
                    {
                        // Check if SalesOrderNum already contains the new sales order number
                        bool existsInSalesOrderNum = equipmentRequest.SalesOrderNum?.Contains(request.RWSalesOrderNum.Replace(";", ",")) ?? false;

                        if (!existsInSalesOrderNum)
                        {
                            // Update EquipmentRequest
                            equipmentRequest.Status = "Sold";
                            equipmentRequest.SalesOrderNum = string.IsNullOrEmpty(equipmentRequest.SalesOrderNum)
                                ? request.RWSalesOrderNum.Replace(";", ",")
                                : $"{equipmentRequest.SalesOrderNum}, {request.RWSalesOrderNum.Replace(";", ",")}";
                            equipmentRequest.SalePrice = detail.UnitPrice;
                            equipmentRequest.MarkedSoldDate = DateTime.Now;
                            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);
                            equipmentRequest.DropShipment = request.DropShipment;

                            await _context.SaveChangesAsync();

                            // Perform PWBQtyRules logic
                            bool shouldMarkAsBought = await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0);

                            if (shouldMarkAsBought && equipmentRequest.Bought != true)
                            {
                                equipmentRequest.Bought = true;
                                await _context.SaveChangesAsync();

                                // Record the history of the item being marked bought
                                var soldItemHistory = new TrkSoldItemHistory
                                {
                                    RequestId = equipmentRequest.RequestId,
                                    Username = request.Username,
                                    PageName = "UpdateSalesOrder"
                                };
                                _context.TrkSoldItemHistories.Add(soldItemHistory);
                                await _context.SaveChangesAsync();
                            }
                            else if (!shouldMarkAsBought && equipmentRequest.Bought == true)
                            {
                                equipmentRequest.Bought = false;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                // Update EquipmentRequests where QtySold = 0 and EventID matches, set Status = 'Lost'
                var equipmentRequestsToUpdate = await _context.EquipmentRequests
                    .Where(er => er.QtySold == 0 && er.EventId == request.EventId)
                    .ToListAsync();

                foreach (var er in equipmentRequestsToUpdate)
                {
                    er.Status = "Lost";
                }
                await _context.SaveChangesAsync();

                // Send notification emails
                await HandleNotificationEmailsAsync(request, salesOrder);

                await transaction.CommitAsync();
                return Ok("Sales order updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error updating SalesOrder: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the sales order.");
            }
        }

        // POST: api/SalesOrderWorkbench/UpdateEquipmentRequest
        [HttpPost("UpdateEquipmentRequest")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrderDetail = await _context.QtSalesOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == request.Id);
                if (salesOrderDetail == null)
                    return NotFound("Sales order detail not found.");

                var equipmentRequest = await _context.EquipmentRequests
                    .FirstOrDefaultAsync(r => r.RequestId == salesOrderDetail.RequestId);
                if (equipmentRequest == null)
                    return NotFound("Equipment request item not found.");

                // Check if SalesOrderNum already contains the new sales order number
                bool existsInSalesOrderNum = equipmentRequest.SalesOrderNum?.Contains(request.RWSalesOrderNum.Replace(";", ",")) ?? false;

                if (!existsInSalesOrderNum)
                {
                    // Update EquipmentRequest
                    equipmentRequest.Status = "Sold";
                    equipmentRequest.SalesOrderNum = string.IsNullOrEmpty(equipmentRequest.SalesOrderNum)
                        ? request.RWSalesOrderNum.Replace(";", ",")
                        : $"{equipmentRequest.SalesOrderNum}, {request.RWSalesOrderNum.Replace(";", ",")}";
                    equipmentRequest.SalePrice = salesOrderDetail.UnitPrice;
                    equipmentRequest.MarkedSoldDate = DateTime.Now;
                    equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (salesOrderDetail.QtySold ?? 0);
                    equipmentRequest.DropShipment = request.DropShipment;

                    // Reset SOFlag
                    salesOrderDetail.Soflag = false;

                    await _context.SaveChangesAsync();

                    // Perform PWBQtyRules logic
                    bool shouldMarkAsBought = await ShouldMarkAsBoughtAsync(equipmentRequest, salesOrderDetail.QtySold ?? 0);

                    if (shouldMarkAsBought && equipmentRequest.Bought != true)
                    {
                        equipmentRequest.Bought = true;
                        await _context.SaveChangesAsync();

                        // Record the history of the item being marked bought
                        var soldItemHistory = new TrkSoldItemHistory
                        {
                            RequestId = equipmentRequest.RequestId,
                            Username = request.Username,
                            PageName = "UpdateEquipmentRequest"
                        };
                        _context.TrkSoldItemHistories.Add(soldItemHistory);
                        await _context.SaveChangesAsync();
                    }
                    else if (!shouldMarkAsBought && equipmentRequest.Bought == true)
                    {
                        equipmentRequest.Bought = false;
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                return Ok("Equipment request updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error updating EquipmentRequest: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the equipment request.");
            }
        }

        // Helper method to determine if the EquipmentRequest should be marked as Bought
        private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest equipmentRequest, int qtySold)
        {
            int QtyOnHand = 0;
            int QtyInPick = 0;
            int Adjustments = 0;
            int QtyFound = 0;
            int NeedToBuy = 0;
            int QtyBought = 0;
            int QtySoldToday = 0;
            int QtyAvailToSell = 0;

            string partNum = equipmentRequest.PartNum;
            string altPartNum = equipmentRequest.AltPartNum;
            int requestId = equipmentRequest.RequestId;
            DateTime quoteDeadline = equipmentRequest.QuoteDeadLine ?? DateTime.Today.AddDays(30);
            int quoteValidFor = Math.Max(0, (quoteDeadline - DateTime.Today).Days);

            // Get QtyOnHand, QtyInPick
            var inventoryItem = await (from r in _context.TrkRwImItems
                                       join i in _context.TrkInventories on r.ItemNum equals i.ItemNum into ii
                                       from i in ii.DefaultIfEmpty()
                                       where i.ItemNum2 == partNum || i.ItemNum == partNum
                                       select new
                                       {
                                           QtyOnHand = r.QtyOnHand ?? 0,
                                           QtyInPick = r.QtyInPick ?? 0
                                       }).FirstOrDefaultAsync();

            if (inventoryItem != null)
            {
                QtyOnHand = inventoryItem.QtyOnHand;
                QtyInPick = inventoryItem.QtyInPick;
            }

            // Get Adjustments
            Adjustments = await (from a in _context.TrkAdjustments
                                 join i in _context.TrkInventories on a.ItemNum equals i.ItemNum
                                 where (i.ItemNum2 == partNum || i.ItemNum == partNum) && a.EntryDate == DateTime.Today
                                 select a.QtyAdjustment).SumAsync() ?? 0;

            // Get QtyBought
            QtyBought = await _context.RequestPos
                .Where(rp => rp.RequestId == requestId)
                .Select(rp => rp.QtyBought)
                .SumAsync() ?? 0;

            // Get QtyFound
            QtyFound = ((int)(await _context.CompetitorCalls
                .Where(cc => (cc.PartNum == partNum || cc.MfgPartNum == altPartNum)
                    && cc.HowMany > 0
                    && cc.ModifiedDate >= DateTime.Now.AddDays(-quoteValidFor)
                    && cc.QtyNotAvailable == false)
                .Select(cc => cc.HowMany)
                .SumAsync() ?? 0));

            // Get QtySoldToday
            QtySoldToday = await _context.EquipmentRequests
                .Where(er => (er.PartNum == partNum || er.AltPartNum == altPartNum)
                    && er.Status == "Sold"
                    && er.MarkedSoldDate.HasValue
                    && er.MarkedSoldDate.Value.Date == DateTime.Today
                    && er.RequestId != requestId)
                .Select(er => er.QtySold)
                .SumAsync() ?? 0;

            // Calculate QtyAvailToSell
            QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;

            // Calculate NeedToBuy
            NeedToBuy = qtySold - QtyAvailToSell - QtyBought;

            // Determine if item should be marked as Bought
            return NeedToBuy <= 0;
        }

        // Helper method to handle notification emails
        private async Task HandleNotificationEmailsAsync(SalesOrderUpdateDto request, QtSalesOrder salesOrder)
        {
            // Check if BillToCompanyName contains 'VERIZON' and send email
            if (!string.IsNullOrEmpty(salesOrder.BillToCompanyName) && salesOrder.BillToCompanyName.ToUpper().Contains("VERIZON"))
            {
                await SendNotificationEmailAsync(request, "sbaker@airway.com", $"A Verizon SO has been assigned to Event ID {request.EventId}", $"SO Number(s): {request.RWSalesOrderNum}");
            }
            else
            {
                // Additional conditions
                var salesOrderInfo = await (from s in _context.QtSalesOrders
                                            join e in _context.EquipmentRequests on s.EventId equals e.EventId into ee
                                            from e in ee.DefaultIfEmpty()
                                            join r in _context.RequestEvents on e.EventId equals r.EventId into rr
                                            from r in rr.DefaultIfEmpty()
                                            join p in _context.RequestPos on e.RequestId equals p.RequestId into pp
                                            from p in pp.DefaultIfEmpty()
                                            join u in _context.Users on p.PurchasedBy equals u.Id into uu
                                            from u in uu.DefaultIfEmpty()
                                            join u2 in _context.Users on r.EventOwner equals u2.Id into uu2
                                            from u2 in uu2.DefaultIfEmpty()
                                            where !s.BillToCompanyName.StartsWith("VERIZON") && s.SaleId == request.SaleId
                                            select new
                                            {
                                                s.BillToCompanyName,
                                                s.Terms,
                                                SalesRep = u2 != null ? u2.Uname : "",
                                                PONum = p != null ? p.Ponum : "",
                                                PORep = u != null ? u.Uname : ""
                                            }).FirstOrDefaultAsync();

                if (salesOrderInfo != null)
                {
                    // Check SO_Email_Notifications
                    bool sendEmailToSbaker = await _context.SoEmailNotifications
                        .AnyAsync(son => son.BillToCompanyName == salesOrderInfo.BillToCompanyName);

                    if (sendEmailToSbaker)
                    {
                        await SendNotificationEmailAsync(request, "sbaker@airway.com", $"A {salesOrderInfo.BillToCompanyName} SO has been assigned to Event ID {request.EventId}", $"SO Number(s): {request.RWSalesOrderNum}");
                    }

                    // Check for prepayment alert
                    if (salesOrderInfo.Terms == "0" && request.DropShipment && salesOrderInfo.PONum != "1234")
                    {
                        var dsSalesRepEmail = $"{salesOrderInfo.SalesRep}@airway.com";
                        var dsPONum = salesOrderInfo.PONum;
                        var dsPurchEmail = !string.IsNullOrEmpty(dsPONum)
                            ? $"{salesOrderInfo.PORep}@airway.com"
                            : "Purch_Dept@airway.com";

                        var recipients = new List<string> { dsSalesRepEmail, dsPurchEmail, "Acct_dept@airway.com" };

                        var emailInput = new EmailInput
                        {
                            FromEmail = "it_department@airway.com",
                            ToEmail = string.Join(",", recipients),
                            Subject = $"PREPAYMENT ALERT FOR SO {request.RWSalesOrderNum.Replace(";", ",")}",
                            HtmlBody = $@"
                                <html><body>
                                    <table>
                                        <tr><td>SO Number: {request.RWSalesOrderNum.Replace(";", ",")} has been flagged as a drop shipment. This is a reminder to collect the prepayment before the item(s) ship.</td></tr>
                                        <tr><td>Sales Rep: {salesOrderInfo.SalesRep}</td></tr>
                                        {(string.IsNullOrEmpty(dsPONum) ? "" : $"<tr><td>PO Number: {dsPONum}</td></tr><tr><td>Purch Rep: {salesOrderInfo.PORep}</td></tr>")}
                                    </table>
                                </body></html>",
                            UserName = request.Username,
                            Password = request.Password,
                        };

                        await _emailService.SendEmailAsync(emailInput);
                    }
                }
            }
        }

        // Helper method to send notification emails
        private async Task SendNotificationEmailAsync(SalesOrderUpdateDto request, string toEmail, string subject, string message)
        {
            var emailInput = new EmailInput
            {
                FromEmail = "it_department@airway.com",
                ToEmail = toEmail,
                Subject = subject,
                HtmlBody = message,
                UserName = request.Username,
                Password = request.Password,
            };

            await _emailService.SendEmailAsync(emailInput);
        }
    }
}
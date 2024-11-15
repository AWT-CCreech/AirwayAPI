using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.UtilityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryRulesController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<InventoryRulesController> _logger;

        public InventoryRulesController(eHelpDeskContext context, ILogger<InventoryRulesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("CalculateQtyRules")]
        public async Task<IActionResult> CalculateQtyRules([FromBody] InventoryRulesRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request data.");
                return BadRequest(ModelState);
            }

            // Initialize variables
            decimal QtyOnHand = 0, QtyInPick = 0, DDQty = 0, Adjustments = 0;
            decimal QtyFound = 0, NeedToFind = 0, NeedToBuy = 0;
            decimal QtyAvail = 0, QtyBought = 0, QtyOnHandCost = 0;
            decimal QtyInStock = 0, QtyToBuyNew = 0, QtySoldToday = 0;
            decimal QtyAvailToSell = 0, QtyToBePickedinMas = 0;
            decimal StoredNeedToBuy = 0, PartialShipDiff = 0;

            try
            {
                _logger.LogInformation("Starting CalculateQtyRules for RequestID: {RequestID}", request.RequestID);

                // Sanitize AltPartNum
                string sanitizedAltPartNum = !string.IsNullOrWhiteSpace(request.AltPartNum)
                    ? request.AltPartNum.Trim().Replace("'", "''")
                    : string.Empty;

                // 1. QUANTITY ON HAND Query using explicit join
                var qohItem = await (from r in _context.TrkRwImItems
                                     join i in _context.TrkInventories
                                         on r.ItemNum equals i.ItemNum
                                     where i.ItemNum2 == request.PartNum.Trim() || i.ItemNum == request.PartNum.Trim()
                                     select new
                                     {
                                         r.QtyOnHand,
                                         r.QtyCost,
                                         TrkUnitCost = i.TrkUnitCost,
                                         r.QtyInPick
                                     }).FirstOrDefaultAsync();

                if (qohItem != null)
                {
                    QtyOnHand = (decimal)qohItem.QtyOnHand;
                    QtyOnHandCost = (decimal)qohItem.QtyCost;
                    QtyToBuyNew = (decimal)qohItem.TrkUnitCost;
                    // QtyInPick is commented out in the original ASP code
                    // QtyInPick = qohItem.QtyInPick;
                    _logger.LogInformation("QtyOnHand: {QtyOnHand}, QtyOnHandCost: {QtyOnHandCost}, QtyToBuyNew: {QtyToBuyNew}", QtyOnHand, QtyOnHandCost, QtyToBuyNew);
                }
                else
                {
                    _logger.LogWarning("No QtyOnHand data found for PartNum: {PartNum}", request.PartNum);
                }

                // 2. QtyToBePickedinMas Query using explicit joins
                var qtyToBePickedinMas = await (from h in _context.TrkRwSoheaders
                                                join d in _context.TrkRwSodetails
                                                    on h.OrderNum equals d.Sonum
                                                join i in _context.TrkInventories
                                                    on d.ItemNum equals i.ItemNum
                                                where h.Status == 0
                                                      && d.QtyOpenToShip > 0
                                                      && h.Type != 3
                                                      && i.ItemNum2 == request.PartNum.Trim().Replace("'", "''")
                                                select d.QtyOpenToShip).SumAsync();

                QtyToBePickedinMas = (decimal)(qtyToBePickedinMas > 0 ? qtyToBePickedinMas : 0);

                QtyInPick += QtyToBePickedinMas;
                _logger.LogInformation("QtyToBePickedinMas: {QtyToBePickedinMas}, QtyInPick after addition: {QtyInPick}", QtyToBePickedinMas, QtyInPick);

                // 3. ADJUSTMENTS Query using explicit join
                var adjustments = await (from a in _context.TrkAdjustments
                                         join i in _context.TrkInventories
                                             on a.ItemNum equals i.ItemNum
                                         where i.ItemNum2 == request.PartNum.Trim()
                                               && a.EntryDate == DateTime.UtcNow.Date
                                         select a.QtyAdjustment).SumAsync();

                Adjustments = (decimal)adjustments;
                _logger.LogInformation("Adjustments: {Adjustments}", Adjustments);

                // 4. QTY BOUGHT FOR THIS Part Number WITHIN x DAYS
                var sumQtyBought = await _context.RequestPos
                    .Where(p => p.RequestId == request.RequestID)
                    .SumAsync(p => (decimal?)p.QtyBought) ?? 0;

                QtyBought = sumQtyBought;
                _logger.LogInformation("QtyBought: {QtyBought}", QtyBought);

                // 5. QTY FOUND using explicit query
                var competitorCallQuery = _context.CompetitorCalls
                    .Where(cc => cc.HowMany > 0
                                 && cc.ModifiedDate > DateTime.UtcNow.AddDays(-request.CallDateRange)
                                 && cc.QtyNotAvailable == false
                                 && ((request.PartNum.Length > 1 && cc.PartNum == request.PartNum.Trim())
                                     || (request.AltPartNum.Length > 1 && cc.MfgPartNum == sanitizedAltPartNum)));

                QtyFound = await competitorCallQuery.SumAsync(cc => (decimal?)cc.HowMany) ?? 0;
                _logger.LogInformation("QtyFound: {QtyFound}", QtyFound);

                // 6. QTY SOLD TODAY
                if (request.RequestID > 0)
                {
                    QtySoldToday = await _context.EquipmentRequests
                        .Where(er => er.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase)
                                     && er.MarkedSoldDate > DateTime.UtcNow.Date
                                     && er.MarkedSoldDate < DateTime.UtcNow
                                     && er.RequestId != request.RequestID
                                     && ((er.PartNum == request.PartNum.Trim())
                                         || (er.AltPartNum == sanitizedAltPartNum)))
                        .SumAsync(er => (decimal?)er.QtySold) ?? 0;
                    _logger.LogInformation("QtySoldToday: {QtySoldToday}", QtySoldToday);
                }

                // 7. Partial Ship Difference using explicit join
                var partialShipDiff = await (from d in _context.TrkRwSodetails
                                             join h in _context.TrkRwSoheaders
                                                 on d.Sonum equals h.OrderNum
                                             where (h.Status == 0 || h.Status == 2)
                                                   && d.QtyShipped > 0
                                                   && d.QtyOrdered > d.QtyShipped
                                                   && d.QtyPicked == 0
                                                   && d.ItemNum == request.PartNum.Trim()
                                             select d.QtyOrdered - d.QtyShipped).SumAsync();

                PartialShipDiff = partialShipDiff ?? 0;
                _logger.LogInformation("PartialShipDiff: {PartialShipDiff}", PartialShipDiff);

                // 8. Calculations
                QtyInStock = QtyOnHand + Adjustments - QtyInPick;
                DDQty = QtyOnHand + Adjustments - QtyInPick + QtyBought;
                QtyAvail = QtyOnHand + QtyFound - QtyBought;
                QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday - PartialShipDiff;

                QtyAvail = QtyAvail < 0 ? 0 : QtyAvail;
                QtyAvailToSell = QtyAvailToSell < 0 ? 0 : QtyAvailToSell;

                _logger.LogInformation("Calculations -> QtyInStock: {QtyInStock}, DDQty: {DDQty}, QtyAvail: {QtyAvail}, QtyAvailToSell: {QtyAvailToSell}", QtyInStock, DDQty, QtyAvail, QtyAvailToSell);

                NeedToFind = request.Quantity - (QtyAvailToSell + QtyFound);
                NeedToFind = NeedToFind < 0 ? 0 : NeedToFind;
                _logger.LogInformation("NeedToFind: {NeedToFind}", NeedToFind);

                // 9. If RequestStatus is "Sold"
                if (request.RequestStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                {
                    NeedToBuy = request.QtySold - QtyAvailToSell - QtyBought;
                    NeedToBuy = NeedToBuy < 0 ? 0 : NeedToBuy;
                    _logger.LogInformation("NeedToBuy: {NeedToBuy}", NeedToBuy);

                    if (NeedToBuy == 0 && request.RequestID > 0)
                    {
                        var equipmentRequest = await _context.EquipmentRequests
                            .FirstOrDefaultAsync(er => er.RequestId == request.RequestID);

                        if (equipmentRequest != null)
                        {
                            equipmentRequest.Bought = true;
                            _context.EquipmentRequests.Update(equipmentRequest);
                            _logger.LogInformation("EquipmentRequest {RequestID} marked as Bought.", request.RequestID);

                            // Record the history
                            var soldItemHistory = new TrkSoldItemHistory
                            {
                                RequestId = request.RequestID,
                                Username = User.Identity.Name ?? "Unknown",
                                PageName = "InventoryQtyRulesController",
                                DateMarkedBought = DateTime.UtcNow
                            };
                            await _context.TrkSoldItemHistories.AddAsync(soldItemHistory);
                            _logger.LogInformation("SoldItemHistory recorded for RequestID: {RequestID}", request.RequestID);
                        }
                        else
                        {
                            _logger.LogWarning("EquipmentRequest with RequestID {RequestID} not found.", request.RequestID);
                        }
                    }
                }

                // 10. Additional Business Logic (e.g., Cable Part Numbers)
                // The original ASP code had some rules related to 'CW' parts, but they were commented out.
                // Implement any additional rules here if necessary.

                // 11. Prepare the response
                var response = new InventoryRulesResponseDto
                {
                    QtyOnHand = QtyOnHand < 0 ? 0 : QtyOnHand,
                    QtyInPick = QtyInPick,
                    DDQty = DDQty,
                    Adjustments = Adjustments,
                    QtyFound = QtyFound,
                    NeedToFind = NeedToFind < 0 ? 0 : NeedToFind,
                    NeedToBuy = NeedToBuy < 0 ? 0 : NeedToBuy,
                    QtyAvail = QtyAvail < 0 ? 0 : QtyAvail,
                    QtyBought = QtyBought,
                    QtyOnHandCost = QtyOnHandCost,
                    QtyInStock = QtyInStock,
                    QtyToBuyNew = QtyToBuyNew,
                    QtySoldToday = QtySoldToday,
                    QtyAvailToSell = QtyAvailToSell < 0 ? 0 : QtyAvailToSell
                };

                // Save changes if any updates occurred
                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Database changes saved.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating quantity rules for RequestID: {RequestID}", request.RequestID);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
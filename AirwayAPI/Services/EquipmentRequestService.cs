using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class EquipmentRequestService : IEquipmentRequestService
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<EquipmentRequestService> _logger;

        public EquipmentRequestService(eHelpDeskContext context, ILogger<EquipmentRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessEquipmentRequest(QtSalesOrderDetail detail, SalesOrderUpdateDto request)
        {
            var equipmentRequest = await _context.EquipmentRequests
                .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId)
                ?? throw new Exception($"EquipmentRequest with RequestID {detail.RequestId} not found.");

            equipmentRequest.SalesOrderNum = UpdateSalesOrderNum(equipmentRequest.SalesOrderNum, request.RWSalesOrderNum);
            equipmentRequest.Status = "Sold";
            equipmentRequest.SalePrice = detail.UnitPrice;
            equipmentRequest.MarkedSoldDate = DateTime.Now;
            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);
            equipmentRequest.DropShipment = request.DropShipment;

            if (await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0))
            {
                equipmentRequest.Bought = true;
                _context.TrkSoldItemHistories.Add(new TrkSoldItemHistory
                {
                    RequestId = equipmentRequest.RequestId,
                    Username = request.Username,
                    PageName = "SalesOrderWorkbenchController(UpdateSalesOrder)"
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<QtSalesOrderDetail> GetSalesOrderDetailByIdAsync(int id)
        {
            try
            {
                var salesOrderDetail = await _context.QtSalesOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (salesOrderDetail == null)
                {
                    throw new Exception($"Sales order detail with ID {id} not found.");
                }

                return salesOrderDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching SalesOrderDetail by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        private string UpdateSalesOrderNum(string existing, string newNum)
        {
            if (string.IsNullOrWhiteSpace(existing)) return newNum;
            if (!existing.Contains(newNum)) return $"{existing}, {newNum}";
            return existing;
        }

        private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest request, int qtySold)
        {
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
                .Where(e => e.PartNum == request.PartNum && e.Status == "Sold" && e.MarkedSoldDate.HasValue && e.MarkedSoldDate.Value.Date == DateTime.Today)
                .SumAsync(e => e.QtySold) ?? 0;

            int QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;
            return qtySold - QtyAvailToSell - QtyBought <= 0;
        }
    }
}

using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;

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
                .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId);

            if (equipmentRequest == null)
                throw new Exception($"EquipmentRequest with RequestID {detail.RequestId} not found.");

            // Update SalesOrderNum
            if (!string.IsNullOrWhiteSpace(equipmentRequest.SalesOrderNum))
            {
                if (!equipmentRequest.SalesOrderNum.Contains(request.RWSalesOrderNum.Replace(";", ",")))
                {
                    equipmentRequest.SalesOrderNum = $"{equipmentRequest.SalesOrderNum}, {request.RWSalesOrderNum.Replace(";", ",")}";
                }
            }
            else
            {
                equipmentRequest.SalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
            }

            // Update other fields
            equipmentRequest.Status = "Sold";
            equipmentRequest.SalePrice = detail.UnitPrice;
            equipmentRequest.MarkedSoldDate = DateTime.Now;
            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);
            equipmentRequest.DropShipment = request.DropShipment;

            // Handle Bought status
            bool shouldMarkAsBought = await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0);

            if (shouldMarkAsBought && equipmentRequest.Bought != true)
            {
                equipmentRequest.Bought = true;
                var history = new TrkSoldItemHistory
                {
                    RequestId = equipmentRequest.RequestId,
                    Username = request.Username,
                    PageName = "SalesOrderWorkbenchController(UpdateSalesOrder)"
                };
                _context.TrkSoldItemHistories.Add(history);
            }
            else if (!shouldMarkAsBought && equipmentRequest.Bought == true)
            {
                equipmentRequest.Bought = false;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest request, int qtySold)
        {
            // Initialize variables
            int QtyOnHand = 0, QtyInPick = 0, Adjustments = 0, QtyBought = 0, QtySoldToday = 0;

            var partNum = request.PartNum;

            // Fetch inventory details
            var inventory = await _context.TrkRwImItems
                .FirstOrDefaultAsync(i => i.ItemNum == partNum || i.AltPartNum == partNum);

            if (inventory != null)
            {
                QtyOnHand = inventory.QtyOnHand ?? 0;
                QtyInPick = inventory.QtyInPick ?? 0;
            }

            // Calculate adjustments
            Adjustments = await _context.TrkAdjustments
                .Where(a => a.ItemNum == partNum && a.EntryDate == DateTime.Today)
                .SumAsync(a => a.QtyAdjustment) ?? 0;

            // Fetch quantities bought
            QtyBought = await _context.RequestPos
                .Where(r => r.RequestId == request.RequestId)
                .SumAsync(r => r.QtyBought) ?? 0;

            // Fetch quantities sold today
            QtySoldToday = await _context.EquipmentRequests
                .Where(e => e.PartNum == partNum && e.Status == "Sold" && e.MarkedSoldDate.HasValue && e.MarkedSoldDate.Value.Date == DateTime.Today)
                .SumAsync(e => e.QtySold) ?? 0;

            // Calculate availability
            int QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;
            int NeedToBuy = qtySold - QtyAvailToSell - QtyBought;

            return NeedToBuy <= 0;
        }

        public async Task<QtSalesOrderDetail> GetSalesOrderDetailByIdAsync(int id)
        {
            return await _context.QtSalesOrderDetails.FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}

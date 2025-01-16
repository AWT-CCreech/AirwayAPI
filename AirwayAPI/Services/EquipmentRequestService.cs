using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class EquipmentRequestService : IEquipmentRequestService
    {
        private readonly eHelpDeskContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<EquipmentRequestService> _logger;

        public EquipmentRequestService(
            eHelpDeskContext context,
            IEmailService emailService,
            ILogger<EquipmentRequestService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ProcessEquipmentRequest(QtSalesOrderDetail detail, SalesOrderUpdateDto request)
        {
            _logger.LogInformation("Processing EquipmentRequest for RequestId {RequestId} with DropShipment: {DropShipment}", detail.RequestId, request.DropShipment);

            var equipmentRequest = await _context.EquipmentRequests
                .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId)
                ?? throw new Exception($"EquipmentRequest with RequestID {detail.RequestId} not found.");

            equipmentRequest.SalesOrderNum = UpdateSalesOrderNum(equipmentRequest.SalesOrderNum ?? "", request.SalesOrderNum);
            equipmentRequest.Status = "Sold";
            equipmentRequest.SalePrice = detail.UnitPrice;
            equipmentRequest.MarkedSoldDate = DateTime.Now;
            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);
            equipmentRequest.DropShipment = request.DropShipment;
            _logger.LogInformation("Updated EquipmentRequest DropShipment to: {DropShipment}", equipmentRequest.DropShipment);

            // Possibly decide if it’s "bought" 
            if (await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0))
            {
                equipmentRequest.Bought = true;
                _context.TrkSoldItemHistories.Add(new TrkSoldItemHistory
                {
                    RequestId = equipmentRequest.RequestId,
                    DateMarkedBought = DateTime.Now,
                    Username = request.Username,
                    PageName = "EquipmentRequestService.cs (ProcessEquipmentRequest)"
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateEquipmentRequestAsync(EquipmentRequestUpdateDto request)
        {
            try
            {
                var detail = await _context.EquipmentRequests
                    .FirstOrDefaultAsync(d => d.RequestId == request.RequestId)
                    ?? throw new Exception($"Request ID#{request.RequestId} not found.");

                detail.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                // Possibly notify via email
                await NotifyEquipmentRequestChanges(request, detail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateEquipmentRequestAsync: {Message}", ex.Message);
                throw;
            }
        }

        private async Task NotifyEquipmentRequestChanges(EquipmentRequestUpdateDto request, EquipmentRequest equipmentRequest)
        {
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);

            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{equipmentRequest.SalesOrderNum} has been updated." },
                { "%%NAME%%", senderInfo.FullName },
                { "%%EMAIL%%", senderInfo.Email },
                { "%%JOBTITLE%%", senderInfo.JobTitle },
                { "%%DIRECT%%", senderInfo.DirectPhone },
                { "%%MOBILE%%", senderInfo.MobilePhone }
            };

            var emailInput = new EmailInputBase
            {
                FromEmail = senderInfo.Email,
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"Sales Order {equipmentRequest.SalesOrderNum} Updated",
                Body = "%%EMAILBODY%%",
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            await _emailService.SendEmailAsync(emailInput);
        }

        private static string UpdateSalesOrderNum(string existing, string newNum)
        {
            // Currently a 1:1 replacement
            return newNum;
        }

        private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest request, int qtySold)
        {
            // Original logic to see if “bought” should be set
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
                .Where(e => e.PartNum == request.PartNum && e.Status == "Sold" &&
                            e.MarkedSoldDate.HasValue && e.MarkedSoldDate.Value.Date == DateTime.Today)
                .SumAsync(e => e.QtySold) ?? 0;

            int QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;
            return (qtySold - QtyAvailToSell - QtyBought) <= 0;
        }
    }
}

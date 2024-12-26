using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class SalesOrderService(
        eHelpDeskContext context,
        IEquipmentRequestService equipmentRequestService,
        IEmailService emailService,
        IQuoteService quoteService,
        IStringService stringService,
        ILogger<SalesOrderService> logger) : ISalesOrderService
    {
        private readonly eHelpDeskContext _context = context;
        private readonly IEquipmentRequestService _equipmentRequestService = equipmentRequestService;
        private readonly IEmailService _emailService = emailService;
        private readonly IQuoteService _quoteService = quoteService;
        private readonly IStringService _stringService = stringService;
        private readonly ILogger<SalesOrderService> _logger = logger;

        public async Task UpdateSalesOrderAsync(SalesOrderUpdateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update Sales Order
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId)
                    ?? throw new Exception($"Sales order with SaleID#{request.SaleId} not found.");

                salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.RWSalesOrderNum);
                salesOrder.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                // Update Related Quote
                await _quoteService.UpdateQuoteAsync(request);

                // Process Related Sales Order Details
                var details = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    await _equipmentRequestService.ProcessEquipmentRequest(detail, request);
                }

                // Notify Changes via Email
                await NotifySalesOrdersChanges(request, salesOrder);

                // Commit Transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateSalesOrderAsync: {ex.Message}", ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateEquipmentRequestAsync(EquipmentRequestUpdateDto request)
        {
            try
            {
                var detail = await _context.EquipmentRequests
                    .FirstOrDefaultAsync(d => d.RequestId == request.RequestId)
                    ?? throw new Exception($"Request ID#{request.RequestId} not found.");

                //detail.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.RWSalesOrderNum);
                detail.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                // Notify Changes via Email
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
            // Retrieve sender information
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);

            // Construct placeholders
            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{equipmentRequest.SalesOrderNum} has been updated." },
                { "%%NAME%%", senderInfo.FullName },
                { "%%EMAIL%%", senderInfo.Email },
                { "%%JOBTITLE%%", senderInfo.JobTitle },
                { "%%DIRECT%%", senderInfo.DirectPhone },
                { "%%MOBILE%%", senderInfo.MobilePhone }
            };

            // Construct email input
            var emailInput = new EmailInputBase
            {
                FromEmail = senderInfo.Email,
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"Sales Order {equipmentRequest.SalesOrderNum} Updated",
                Body = "%%EMAILBODY%%", // Placeholder to be replaced dynamically
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            // Send email
            await _emailService.SendEmailAsync(emailInput);
        }


        private async Task NotifySalesOrdersChanges(SalesOrderUpdateDto request, QtSalesOrder salesOrder)
        {
            // Retrieve sender information
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);

            // Construct placeholders
            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{salesOrder.RwsalesOrderNum} has been updated." },
                { "%%NAME%%", senderInfo.FullName },
                { "%%EMAIL%%", senderInfo.Email },
                { "%%JOBTITLE%%", senderInfo.JobTitle },
                { "%%DIRECT%%", senderInfo.DirectPhone },
                { "%%MOBILE%%", senderInfo.MobilePhone }
            };

            // Construct email input
            var emailInput = new EmailInputBase
            {
                FromEmail = senderInfo.Email,
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"Sales Order {salesOrder.RwsalesOrderNum} Updated",
                Body = "%%EMAILBODY%%", // Placeholder to be replaced dynamically
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            // Send email
            await _emailService.SendEmailAsync(emailInput);
        }
    }
}
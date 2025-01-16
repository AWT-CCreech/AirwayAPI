using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly eHelpDeskContext _context;
        private readonly IEquipmentRequestService _equipmentRequestService;
        private readonly IEmailService _emailService;
        private readonly IQuoteService _quoteService;
        private readonly IStringService _stringService;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(
            eHelpDeskContext context,
            IEquipmentRequestService equipmentRequestService,
            IEmailService emailService,
            IQuoteService quoteService,
            IStringService stringService,
            ILogger<SalesOrderService> logger)
        {
            _context = context;
            _equipmentRequestService = equipmentRequestService;
            _emailService = emailService;
            _quoteService = quoteService;
            _stringService = stringService;
            _logger = logger;
        }

        public async Task UpdateSalesOrderAsync(SalesOrderUpdateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Fetch the SalesOrder
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId)
                    ?? throw new Exception($"Sales order with SaleID#{request.SaleId} not found.");

                // Update with new data
                salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
                salesOrder.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                // Update related Quote
                await _quoteService.UpdateQuoteAsync(request);

                // Process associated SalesOrderDetails 
                var details = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    await _equipmentRequestService.ProcessEquipmentRequest(detail, request);
                }

                // Possibly: email notifications
                await NotifySalesOrderChanges(request, salesOrder);

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateSalesOrderAsync Transaction rolled back due to error: {Message}", ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QtSalesOrderDetail> GetSalesOrderDetailByIdAsync(int id)
        {
            var detail = await _context.QtSalesOrderDetails
                .FirstOrDefaultAsync(d => d.RequestId == id);

            return detail ?? throw new Exception($"Sales order detail with ID {id} not found.");
        }

        private async Task NotifySalesOrderChanges(SalesOrderUpdateDto request, QtSalesOrder salesOrder)
        {
            // Gather email placeholders, etc.
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);
            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{salesOrder.RwsalesOrderNum} has been updated." },
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
                Subject = $"Sales Order {salesOrder.RwsalesOrderNum} Updated",
                Body = "%%EMAILBODY%%",
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            // Send
            await _emailService.SendEmailAsync(emailInput);
        }
    }
}

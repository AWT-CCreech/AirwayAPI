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
                await NotifyChanges(request, salesOrder);

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

        private async Task NotifyChanges(SalesOrderUpdateDto request, QtSalesOrder salesOrder)
        {
            string subject = $"Sales Order {salesOrder.RwsalesOrderNum} Updated";
            string body = $"The sales order {salesOrder.RwsalesOrderNum} has been updated.";

            await _emailService.SendEmailAsync(new EmailInputBase
            {
                FromEmail = $"{request.Username}@airway.com",
                ToEmails = ["ccreech@airway.com"],
                Subject = subject,
                Body = body,
                UserName = request.Username,
                Password = request.Password
            });
        }
    }
}
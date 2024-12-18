using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services;
using Microsoft.EntityFrameworkCore;

public class SalesOrderService : ISalesOrderService
{
    private readonly eHelpDeskContext _context;
    private readonly IEquipmentRequestService _equipmentRequestService;
    private readonly IEmailService _emailService;
    private readonly IQuoteService _quoteService;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(
        eHelpDeskContext context,
        IEquipmentRequestService equipmentRequestService,
        IEmailService emailService,
        IQuoteService quoteService,
        ILogger<SalesOrderService> logger)
    {
        _context = context;
        _equipmentRequestService = equipmentRequestService;
        _emailService = emailService;
        _quoteService = quoteService;
        _logger = logger;
    }

    public async Task UpdateSalesOrderAsync(SalesOrderUpdateDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update Sales Order
            var salesOrder = await _context.QtSalesOrders
                .FirstOrDefaultAsync(so => so.SaleId == request.SaleId)
                ?? throw new Exception($"Sales order with SaleID#{request.SaleId} not found.");

            salesOrder.RwsalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
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
        string subject = $"Sales Order {request.RWSalesOrderNum} Updated";
        string body = $"The sales order {request.RWSalesOrderNum} has been updated.";

        await _emailService.SendEmailAsync(new EmailInputBase
        {
            FromEmail = $"{request.Username}@airway.com",
            ToEmails = new List<string> { "ccreech@airway.com" },
            Subject = subject,
            Body = body,
            UserName = request.Username,
            Password = request.Password
        });
    }
}

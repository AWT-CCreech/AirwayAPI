using AirwayAPI.Data;
using AirwayAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AirwayAPI.Services
{
    public class SalesOrderService(
        eHelpDeskContext context,
        IEquipmentRequestService equipmentRequestService,
        IEmailService emailService,
        ILogger<SalesOrderService> logger) : ISalesOrderService
    {
        private readonly eHelpDeskContext _context = context;
        private readonly IEquipmentRequestService _equipmentRequestService = equipmentRequestService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<SalesOrderService> _logger = logger;

        public async Task UpdateSalesOrderAsync(SalesOrderUpdateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId) ?? throw new Exception($"Sales order with SaleId {request.SaleId} not found.");

                // Update Sales Order
                salesOrder.RwsalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
                salesOrder.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                // Fetch related Sales Order Details
                var details = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    await _equipmentRequestService.ProcessEquipmentRequest(detail, request);
                }

                // EMAIL NOTIFICATIONS

                // EMAIL for Verizon SO
                bool isVerizon = salesOrder.BillToCompanyName.StartsWith("VERIZON");

                if (isVerizon)
                {
                    await _emailService.SendEmailAsync(new EmailInputDto
                    {
                        FromEmail = "ITDept@airway.com",
                        ToEmail = "ccreech@airway.com",
                        Subject = $"A Verizon SO has been assigned to Event ID {request.EventId}",
                        HtmlBody = $"SO Number(s): {request.RWSalesOrderNum}"
                    });
                }

                // EMAIL for non-Verizon Contractors and Drop Shipments
                if (!isVerizon)
                {
                    // Check if there are email notifications configured for this BillToCompanyName
                    bool hasEmailNotification = await _context.SoEmailNotifications
                        .AnyAsync(s => s.BillToCompanyName == salesOrder.BillToCompanyName);

                    if (hasEmailNotification)
                    {
                        await _emailService.SendEmailAsync(new EmailInputDto
                        {
                            FromEmail = "ITDept@airway.com",
                            ToEmail = "ccreech@airway.com",
                            Subject = $"A {salesOrder.BillToCompanyName} SO has been assigned to Event ID {request.EventId}",
                            HtmlBody = $"SO Number(s): {request.RWSalesOrderNum}"
                        });
                    }

                    // SEND DROPSHIP EMAIL TO ACCT/PURCH IF TRUE
                    if (salesOrder.Terms == "0" && request.DropShipment && salesOrder.CustomerPo != "1234")
                    {
                        // Fetch SalesRep and PORep emails
                        var salesRepEmail = await _context.Users
                            .Where(u => u.Id == salesOrder.AccountMgr)
                            .Select(u => u.Uname + "@airway.com")
                            .FirstOrDefaultAsync();

                        var poRepEmail = string.IsNullOrEmpty(salesOrder.CustomerPo)
                            ? "Purch_Dept@airway.com"
                            : await _context.Users
                                .Where(u => u.Id == salesOrder.AccountMgr) // Adjust if PORep is different
                                .Select(u => u.Uname + "@airway.com")
                                .FirstOrDefaultAsync();

                        var ccEmails = new List<string>
                        {
                            "ccreech@airway.com",
                            salesRepEmail,
                            poRepEmail,
                            "Acct_dept@airway.com"
                        }.Where(email => !string.IsNullOrWhiteSpace(email)).ToList();

                        string subject = $"PREPAYMENT ALERT FOR SO {salesOrder.RwsalesOrderNum}";
                        string body = $@"
                            <html>
                                <body>
                                    <table>
                                        <tr><td>SO Number: {salesOrder.RwsalesOrderNum} has been flagged as a drop shipment. This is a reminder to collect the prepayment before the item(s) ship.</td></tr>
                                        <tr><td>Sales Rep: {salesRepEmail?.Split('@')[0]}</td></tr>
                                        {(string.IsNullOrEmpty(salesOrder.CustomerPo) ? "" : $"<tr><td>PO Number: {salesOrder.CustomerPo}</td></tr><tr><td>Purch Rep: {poRepEmail?.Split('@')[0]}</td></tr>")}
                                    </table>
                                </body>
                            </html>";

                        foreach (var email in ccEmails)
                        {
                            await _emailService.SendEmailAsync(new EmailInputDto
                            {
                                FromEmail = "ITDept@airway.com",
                                ToEmail = email,
                                Subject = subject,
                                HtmlBody = body,
                                Urgent = true
                            });
                        }
                    }
                }

                // Mark other Request items for the Event as LOST if QtySold = 0
                var eventId = request.EventId;
                var lostEquipmentRequests = _context.EquipmentRequests
                    .Where(er => er.EventId == eventId && er.QtySold == 0);

                foreach (var lostEr in lostEquipmentRequests)
                {
                    lostEr.Status = "Lost";
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateSalesOrderAsync: {ex.Message}", ex);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

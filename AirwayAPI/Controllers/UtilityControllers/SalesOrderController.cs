using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.ServiceModels;
using AirwayAPI.Models.UtilityModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController(
        eHelpDeskContext context,
        ILogger<SalesOrderController> logger,
        EquipmentRequestService equipmentRequestService,
        EmailService emailService) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<SalesOrderController> _logger = logger;
        private readonly EquipmentRequestService _equipmentRequestService = equipmentRequestService;
        private readonly EmailService _emailService = emailService;

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId);

                if (salesOrder == null)
                    return NotFound("Sales order not found.");

                salesOrder.RwsalesOrderNum = request.RWSalesOrderNum.Replace(";", ",");
                salesOrder.DropShipment = request.DropShipment;

                await _context.SaveChangesAsync();

                var details = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    await _equipmentRequestService.ProcessEquipmentRequest(detail, request);
                }

                await transaction.CommitAsync();

                // Send notification email
                await _emailService.SendEmailAsync(new EmailInput
                {
                    FromEmail = "no-reply@airway.com",
                    ToEmail = "notification@airway.com",
                    Subject = "Sales Order Updated",
                    HtmlBody = $"Sales order {request.RWSalesOrderNum} has been updated successfully."
                });

                return Ok("Sales order updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error updating sales order: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the sales order.");
            }
        }
    }
}

using AirwayAPI.Data;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentRequestController(
        IEquipmentRequestService equipmentRequestService,
        ILogger<EquipmentRequestController> logger,
        eHelpDeskContext context) : ControllerBase
    {
        private readonly IEquipmentRequestService _equipmentRequestService = equipmentRequestService;
        private readonly ILogger<EquipmentRequestController> _logger = logger;
        private readonly eHelpDeskContext _context = context;

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Fetch the SalesOrderDetail by Id
                var detail = await _equipmentRequestService.GetSalesOrderDetailByIdAsync(request.Id);
                if (detail == null)
                    return NotFound($"QtSalesOrderDetail with Id {request.Id} not found.");

                // Create SalesOrderUpdateDto based on EquipmentRequestUpdateDto
                var salesOrderUpdateDto = new SalesOrderUpdateDto
                {
                    SaleId = detail.SaleId ?? 0, // Ensure SaleId is not null
                    EventId = 0, // To be set based on related SalesOrder
                    QuoteId = 0, // To be set based on related SalesOrder
                    RWSalesOrderNum = request.RWSalesOrderNum,
                    DropShipment = request.DropShipment,
                    Username = request.Username
                };

                // Fetch the related SalesOrder to set EventId and QuoteId
                var salesOrder = await _context.QtSalesOrders.FirstOrDefaultAsync(so => so.SaleId == salesOrderUpdateDto.SaleId);
                if (salesOrder != null)
                {
                    salesOrderUpdateDto.EventId = salesOrder.EventId ?? 0;
                    salesOrderUpdateDto.QuoteId = salesOrder.QuoteId ?? 0;
                }

                await _equipmentRequestService.ProcessEquipmentRequest(detail, salesOrderUpdateDto);

                return Ok("Equipment request updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating Equipment Request: {ex.Message}", ex);
                return StatusCode(500, "Error updating Equipment Request");
            }
        }
    }
}

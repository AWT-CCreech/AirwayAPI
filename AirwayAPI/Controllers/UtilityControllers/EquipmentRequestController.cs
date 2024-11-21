using AirwayAPI.Data;
using AirwayAPI.Models;
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
    public class EquipmentRequestController(
        eHelpDeskContext context,
        ILogger<EquipmentRequestController> logger,
        EquipmentRequestService equipmentRequestService) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<EquipmentRequestController> _logger = logger;
        private readonly EquipmentRequestService _equipmentRequestService = equipmentRequestService;

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var detail = await _context.QtSalesOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == request.Id);
                if (detail == null)
                    return NotFound("Sales order detail not found.");

                await _equipmentRequestService.ProcessEquipmentRequest(detail, new SalesOrderUpdateDto
                {
                    SaleId = (int)detail.SaleId,
                    RWSalesOrderNum = request.RWSalesOrderNum,
                    Username = request.Username,
                    DropShipment = request.DropShipment
                });

                await transaction.CommitAsync();
                return Ok("Equipment request updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error updating Equipment Request: {ex.Message}");
                return StatusCode(500, "Error updating Equipment Request");
            }
        }
    }
}

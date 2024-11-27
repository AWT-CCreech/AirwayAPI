using AirwayAPI.Models.DTOs;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController(ISalesOrderService salesOrderService, ILogger<SalesOrderController> logger) : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService = salesOrderService;
        private readonly ILogger<SalesOrderController> _logger = logger;

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _salesOrderService.UpdateSalesOrderAsync(request);
                return Ok("Sales order updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating sales order: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the sales order.");
            }
        }
    }
}

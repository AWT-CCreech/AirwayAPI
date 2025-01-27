using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirwayAPI.Services.Interfaces;
using AirwayAPI.Models.DTOs;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController(
        ISalesOrderWorkbenchService workbenchService,
        ILogger<SalesOrderWorkbenchController> logger) : ControllerBase
    {
        private readonly ISalesOrderWorkbenchService _workbenchService = workbenchService;
        private readonly ILogger<SalesOrderWorkbenchController> _logger = logger;

        #region 1) GET methods (Event-Level & Detail-Level)
        /// <summary>
        /// Get the event-level data for a given sales order.
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <param name="billToCompany"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData(
            [FromQuery] int? salesRepId,
            [FromQuery] string? billToCompany,
            [FromQuery] int? eventId)
        {
            try
            {
                var results = await _workbenchService.GetEventLevelDataAsync(salesRepId, billToCompany, eventId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetEventLevelData: {Message}", ex.Message);
                return StatusCode(500, "Error fetching Event Level Data");
            }
        }

        /// <summary>
        /// Get the detail-level data for a given sales order.
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <param name="billToCompany"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData(
            [FromQuery] int? salesRepId,
            [FromQuery] string? billToCompany,
            [FromQuery] int? eventId)
        {
            try
            {
                var results = await _workbenchService.GetDetailLevelDataAsync(salesRepId, billToCompany, eventId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetDetailLevelData: {Message}", ex.Message);
                return StatusCode(500, "Error fetching Detail Level Data");
            }
        }
        #endregion

        #region 2) POST methods (UpdateEventLevel & UpdateDetailLevel)
        /// <summary>
        /// Update the event-level data for a given sales order.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("UpdateEventLevel")]
        public async Task<IActionResult> UpdateEventLevel([FromBody] SalesOrderUpdateDto request)
        {
            _logger.LogInformation("UpdateEventLevel called with: {@Request}", request);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _workbenchService.UpdateEventLevelAsync(request);
                return Ok("Event-level update completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateEventLevel: {Message}", ex.Message);
                return StatusCode(500, "Error updating event-level data.");
            }
        }

        /// <summary>
        /// Update the detail-level data for a given sales order.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("UpdateDetailLevel")]
        public async Task<IActionResult> UpdateDetailLevel([FromBody] EquipmentRequestUpdateDto request)
        {
            _logger.LogInformation("UpdateDetailLevel called with: {@Request}", request);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _workbenchService.UpdateDetailLevelAsync(request);
                return Ok("Detail-level update completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UpdateDetailLevel: {Message}", ex.Message);
                return StatusCode(500, "Error updating detail-level data.");
            }
        }
        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirwayAPI.Services.Interfaces;
using AirwayAPI.Models.SalesOrderWorkbenchModels;

namespace AirwayAPI.Controllers.SalesOrderWorkbenchControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController(
        ISalesOrderService workbenchService,
        ILogger<SalesOrderWorkbenchController> logger) : ControllerBase
    {
        private readonly ISalesOrderService _workbenchService = workbenchService;
        private readonly ILogger<SalesOrderWorkbenchController> _logger = logger;

        #region 1) GET endpoints (Event-Level & Detail-Level)

        /// <summary>
        /// Retrieves the Event-level data (no MAS # yet).
        /// </summary>
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
        /// Retrieves the Detail-level data (SOFlag=1, no MAS # yet).
        /// </summary>
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

        #region 2) POST endpoints (UpdateEventLevel & UpdateDetailLevel)

        /// <summary>
        /// Assign or update the MAS SO # at the “Event” level.
        /// This is called in a loop by the React front end for each row the user updates.
        /// </summary>
        [HttpPost("UpdateEventLevel")]
        public async Task<IActionResult> UpdateEventLevel([FromBody] EventLevelUpdateDto request)
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
        /// Assign or update the MAS SO # at the “Detail” level.
        /// This is called in a loop by the React front end for each detail row the user updates.
        /// </summary>
        [HttpPost("UpdateDetailLevel")]
        public async Task<IActionResult> UpdateDetailLevel([FromBody] DetailLevelUpdateDto request)
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

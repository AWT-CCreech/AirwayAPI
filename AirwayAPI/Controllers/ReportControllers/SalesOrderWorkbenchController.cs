using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController : ControllerBase
    {
        private readonly ISalesOrderWorkbenchService _workbenchService;
        private readonly ILogger<SalesOrderWorkbenchController> _logger;

        public SalesOrderWorkbenchController(
            ISalesOrderWorkbenchService workbenchService,
            ILogger<SalesOrderWorkbenchController> logger)
        {
            _workbenchService = workbenchService;
            _logger = logger;
        }

        // GET: api/SalesOrderWorkbench/EventLevelData
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

        // GET: api/SalesOrderWorkbench/DetailLevelData
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
    }
}

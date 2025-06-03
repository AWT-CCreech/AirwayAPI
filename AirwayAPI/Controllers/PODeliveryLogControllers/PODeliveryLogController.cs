using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.PODeliveryLogControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PODeliveryLogController(
    IPurchasingService service,
    ILogger<PODeliveryLogController> logger) : ControllerBase
{
    private readonly IPurchasingService _service = service;
    private readonly ILogger<PODeliveryLogController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetPODeliveryLogs([FromQuery] PODeliveryLogQueryParameters parameters)
    {
        var results = await _service.GetPODeliveryLogsAsync(parameters);
        return Ok(results);
    }

    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors([FromQuery] PODeliveryLogQueryParameters parameters)
    {
        var vendors = await _service.GetVendorsAsync(parameters);
        return Ok(vendors);
    }
}
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.UtilityControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OperationsController(
    ILogger<OperationsController> logger,
    IUserService userService) : ControllerBase
{
    private readonly ILogger<OperationsController> _logger = logger;
    private readonly IUserService _userService = userService;

    /// <summary>
    /// GET api/Operations/warehouse
    /// Returns all active warehouse users (DeptID = 8) with at least one scan.
    /// </summary>
    [HttpGet("ScanUsers")]
    public async Task<IActionResult> GetScanUsers()
    {
        var users = await _userService.GetScanUsersAsync();
        return Ok(users);
    }
}
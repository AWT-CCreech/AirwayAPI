using AirwayAPI.Models.DTOs;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentRequestController : ControllerBase
    {
        private readonly IEquipmentRequestService _equipmentRequestService;
        private readonly ILogger<EquipmentRequestController> _logger;

        public EquipmentRequestController(
            IEquipmentRequestService equipmentRequestService,
            ILogger<EquipmentRequestController> logger)
        {
            _equipmentRequestService = equipmentRequestService;
            _logger = logger;
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _equipmentRequestService.UpdateEquipmentRequestAsync(request);
                return Ok("Equipment request updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating equipment request: {Message}", ex.Message);
                return StatusCode(500, "Error updating equipment request.");
            }
        }
    }
}

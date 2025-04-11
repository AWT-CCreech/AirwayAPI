using AirwayAPI.Models.ScanHistoryModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.ScanControllers;

[ApiController]
[Route("api/[controller]")]
public class ScanHistoryController(IScanService scanService, ILogger<ScanHistoryController> logger) : ControllerBase
{
    private readonly IScanService _scanService = scanService;
    private readonly ILogger<ScanHistoryController> _logger = logger;

    /// <summary>
    /// Retrieves scan history records based on query parameters.
    /// </summary>
    /// <param name="searchDto">Search criteria for scan history.</param>
    [HttpGet("Search")]
    public async Task<IActionResult> SearchScans([FromQuery] SearchScansDto searchDto)
    {
        try
        {
            var scans = await _scanService.SearchScanHistoryAsync(searchDto);
            return Ok(scans);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error searching scan history: {Message}", ex.Message);
            return StatusCode(500, "Error retrieving scan history.");
        }
    }

    /// <summary>
    /// Deletes the specified scan history records.
    /// </summary>
    /// <param name="selectedIds">A collection of RowIds to delete.</param>
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteScans([FromBody] IEnumerable<int> selectedIds)
    {
        try
        {
            int deletedCount = await _scanService.DeleteScansAsync(selectedIds);
            return Ok(new { DeletedCount = deletedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting scans: {Message}", ex.Message);
            return StatusCode(500, "Error deleting scan histories.");
        }
    }

    /// <summary>
    /// Updates the specified scan history records.
    /// </summary>
    /// <param name="updateDtos">A collection of update data transfer objects.</param>
    [HttpPut("Update")]
    public async Task<IActionResult> UpdateScans([FromBody] IEnumerable<UpdateScanDto> updateDtos)
    {
        try
        {
            int updatedCount = await _scanService.UpdateScansAsync(updateDtos);
            return Ok(new { UpdatedCount = updatedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating scans: {Message}", ex.Message);
            return StatusCode(500, "Error updating scan histories.");
        }
    }

    /// <summary>
    /// Copies the selected scans from one order to another.
    /// </summary>
    /// <param name="copyRequest">Data transfer object containing copy parameters and IDs.</param>
    [HttpPost("Copy")]
    public async Task<IActionResult> CopyScans([FromBody] CopyScansDto copyRequest)
    {
        try
        {
            int copiedCount = await _scanService.CopyScansAsync(copyRequest);
            return Ok(new { CopiedCount = copiedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error copying scans: {Message}", ex.Message);
            return StatusCode(500, "Error copying scans.");
        }
    }

    /// <summary>
    /// Adds the selected scan records to the Test Lab.
    /// </summary>
    /// <param name="selectedIds">A collection of RowIds whose scans should be added to test lab.</param>
    [HttpPost("AddTestScans")]
    public async Task<IActionResult> AddTestScans([FromBody] IEnumerable<int> selectedIds)
    {
        try
        {
            int addedCount = await _scanService.AddTestLabScansAsync(selectedIds);
            return Ok(new { AddedCount = addedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error adding scans to test lab: {Message}", ex.Message);
            return StatusCode(500, "Error adding scans to test lab.");
        }
    }
}

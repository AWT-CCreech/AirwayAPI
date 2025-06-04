using AirwayAPI.Models.GenericDtos;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.PODeliveryLogControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PODetailController(IPurchasingService purchasingService) : ControllerBase
{
    private readonly IPurchasingService _purchasingService = purchasingService;

    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetPODetailByID(int id)
    {
        try
        {
            var dto = await _purchasingService.GetPODetailByIdAsync(id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePODetail(int id, [FromBody] PODetailUpdateDto updateDto)
    {
        try
        {
            await _purchasingService.UpdatePODetailAsync(id, updateDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/note")]
    public async Task<IActionResult> AddNoteToPODetail(int id, [FromBody] NoteDto noteDto)
    {
        try
        {
            await _purchasingService.AddNoteAsync(id, noteDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
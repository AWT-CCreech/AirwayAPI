﻿using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.OpenSalesOrderControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OpenSalesOrderNotesController(eHelpDeskContext context) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;

    [HttpGet("GetNotes/{soNum}/{partNum}")]
    public async Task<IActionResult> GetNotes(string soNum, string partNum)
    {
        var notes = await _context.TrkSonotes
            .Where(n => n.OrderNo == soNum && n.PartNo == partNum)
            .Select(n => new
            {
                n.Id,
                n.Notes,
                n.ContactId,
                ContactName = _context.CamContacts
                                   .Where(c => c.Id == n.ContactId)
                                   .Select(c => c.Contact)
                                   .FirstOrDefault(),
                n.EntryDate,
                n.EnteredBy
            })
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost("AddNote")]
    public async Task<IActionResult> AddNote([FromBody] TrkSonote note)
    {
        if (note == null)
        {
            return BadRequest("Note cannot be null");
        }

        _context.TrkSonotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotes), new { soNum = note.OrderNo, partNum = note.PartNo }, note);
    }

    [HttpPut("UpdateNote/{id}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] TrkSonote note)
    {
        if (id != note.Id)
        {
            return BadRequest();
        }

        _context.Entry(note).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("DeleteNote/{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var note = await _context.TrkSonotes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        _context.TrkSonotes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

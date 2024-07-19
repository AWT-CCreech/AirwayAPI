using Microsoft.AspNetCore.Mvc;
using AirwayAPI.Data;
using AirwayAPI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenSONotesController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public OpenSONotesController(eHelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet("{soNum}/{partNum}")]
        public async Task<IActionResult> GetNotes(string soNum, string partNum)
        {
            var notes = await _context.TrkSonotes
                .Where(n => n.OrderNo == soNum && n.PartNo == partNum)
                .Select(n => n.Notes)
                .ToListAsync();

            return Ok(notes);
        }

        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] TrkSonote note)
        {
            _context.TrkSonotes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotes), new { soNum = note.OrderNo, partNum = note.PartNo }, note);
        }

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
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
}

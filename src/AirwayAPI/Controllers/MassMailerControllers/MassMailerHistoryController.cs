using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MassMailerControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MassMailerHistoryController(eHelpDeskContext context, IUserService userService) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;
    private readonly IUserService _userService = userService;

    // GET: api/MassMailerHistory
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MassMailHistory>>> GetMassMailHistories()
    {
        return await _context.MassMailHistories.ToListAsync();
    }

    // GET: api/MassMailerHistory/sentBy/ccreech
    [HttpGet("sentBy/{username}")]
    public async Task<ActionResult<IEnumerable<MassMailHistory>>> GetMassMailHistoriesByUser(string username)
    {
        // Use the UserService to convert the username to its user ID.
        int userId = await _userService.GetUserIdAsync(username);
        if (userId <= 0)
        {
            return NotFound($"User '{username}' not found.");
        }

        DateTime now = DateTime.Now;
        DateTime twoYearsAgo = now.AddYears(-2);
        // Join MassMailers with MassMailHistories and filter by the user ID.
        List<MassMailHistory> histories = await (from history in _context.MassMailHistories
                                                 join mail in _context.MassMailers
                                                   on history.MassMailId equals mail.MassMailId
                                                 where mail.SentBy.HasValue && mail.SentBy.Value == userId && mail.DateSent > twoYearsAgo
                                                 orderby history.Id descending
                                                 select history)
                               .ToListAsync();

        if (histories == null || histories.Count == 0)
        {
            return NotFound("No history records found for the given user.");
        }

        return histories;
    }

    // GET: api/MassMailerHistory/sentBy/ccreech/5
    [HttpGet("sentBy/{username}/{id}")]
    public async Task<ActionResult<MassMailHistory>> GetMassMailHistory(string username, int id)
    {
        int userId = await _userService.GetUserIdAsync(username);
        if (userId <= 0)
        {
            return NotFound($"User '{username}' not found.");
        }

        var history = await (from h in _context.MassMailHistories
                             join m in _context.MassMailers on h.MassMailId equals m.MassMailId
                             where m.SentBy.HasValue && m.SentBy.Value == userId &&
                                   h.Id == id
                             select h)
                            .FirstOrDefaultAsync();

        if (history == null)
        {
            return NotFound();
        }

        return history;
    }

    // PUT: api/MassMailerHistory/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMassMailHistory(int id, MassMailHistory massMailHistory)
    {
        if (id != massMailHistory.Id)
        {
            return BadRequest("The provided ID does not match the record's ID.");
        }

        _context.Entry(massMailHistory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MassMailHistoryExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/MassMailerHistory
    [HttpPost]
    public async Task<ActionResult<MassMailHistory>> PostMassMailHistory(MassMailHistory massMailHistory)
    {
        _context.MassMailHistories.Add(massMailHistory);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (MassMailHistoryExists(massMailHistory.Id))
            {
                return Conflict();
            }
            else
            {
                throw;
            }
        }

        // Depending on routing preferences might adjust the action name.
        return CreatedAtAction("GetMassMailHistories", new { id = massMailHistory.Id }, massMailHistory);
    }

    // DELETE: api/MassMailerHistory/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<MassMailHistory>> DeleteMassMailHistory(int id)
    {
        var massMailHistory = await _context.MassMailHistories.FindAsync(id);
        if (massMailHistory == null)
        {
            return NotFound();
        }

        _context.MassMailHistories.Remove(massMailHistory);
        await _context.SaveChangesAsync();

        return massMailHistory;
    }

    private bool MassMailHistoryExists(int id)
    {
        return _context.MassMailHistories.Any(e => e.Id == id);
    }
}

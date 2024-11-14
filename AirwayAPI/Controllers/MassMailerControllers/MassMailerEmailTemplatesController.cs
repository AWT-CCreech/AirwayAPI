using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerEmailTemplatesController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        // GET: api/MassMailerEmailTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CamCannedEmail>>> GetCamCannedEmails()
        {
            return await _context.CamCannedEmails.ToListAsync();
        }

        // GET: api/MassMailerEmailTemplates/ccreech
        [HttpGet("{user}")]
        public async Task<ActionResult<IEnumerable<CamCannedEmail>>> GetCamCannedEmails(string user)
        {
            var normalizedUser = user.Trim().ToLower();
            var templatesForUser = await _context.CamCannedEmails
                .Where(email => email.EnteredBy != null && email.EnteredBy.Trim().ToLower() == normalizedUser)
                .ToListAsync();

            if (!templatesForUser.Any())
                return NotFound();

            return templatesForUser;
        }

        // GET: api/MassMailerEmailTemplates/ccreech/5
        [HttpGet("{user}/{id}")]
        public async Task<ActionResult<CamCannedEmail>> GetCamCannedEmail(string user, int id)
        {
            var normalizedUser = user.Trim().ToLower();
            var templateForUserWithId = await _context.CamCannedEmails
                .FirstOrDefaultAsync(email => email.EnteredBy != null &&
                    email.EnteredBy.Trim().ToLower() == normalizedUser &&
                    email.Id == id);

            if (templateForUserWithId == null)
                return NotFound();

            return templateForUserWithId;
        }

        // PUT: api/MassMailerEmailTemplates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamCannedEmail(int id, CamCannedEmail camCannedEmail)
        {
            if (id != camCannedEmail.Id)
                return BadRequest();

            _context.Entry(camCannedEmail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CamCannedEmailExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/MassMailerEmailTemplates
        [HttpPost]
        public async Task<ActionResult<CamCannedEmail>> PostCamCannedEmail(CamCannedEmail camCannedEmail)
        {
            _context.CamCannedEmails.Add(camCannedEmail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CamCannedEmailExists(camCannedEmail.Id))
                    return Conflict();
                else
                    throw;
            }

            return CreatedAtAction("GetCamCannedEmails", new { id = camCannedEmail.Id }, camCannedEmail);
        }

        // DELETE: api/MassMailerEmailTemplates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CamCannedEmail>> DeleteCamCannedEmail(int id)
        {
            var camCannedEmail = await _context.CamCannedEmails.FindAsync(id);
            if (camCannedEmail == null)
                return NotFound();

            _context.CamCannedEmails.Remove(camCannedEmail);
            await _context.SaveChangesAsync();

            return camCannedEmail;
        }

        private bool CamCannedEmailExists(int id)
        {
            return _context.CamCannedEmails.Any(e => e.Id == id);
        }
    }
}
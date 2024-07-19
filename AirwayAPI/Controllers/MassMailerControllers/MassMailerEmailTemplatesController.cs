using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerEmailTemplatesController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerEmailTemplatesController(eHelpDeskContext context)
        {
            _context = context;
        }

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
            var templatesForUser = await _context.CamCannedEmails
                .Where(email => email.EnteredBy != null && email.EnteredBy.Trim().ToLower() == user.Trim().ToLower())
                .ToListAsync();

            if (templatesForUser == null)
                return NotFound();

            return templatesForUser;
        }
        
        // GET: api/MassMailerEmailTemplates/5
        [HttpGet("{user}/{id}")]
        public ActionResult<CamCannedEmail> GetCamCannedEmails(string user, int id)
        {
            var templateForUserWithId = _context.CamCannedEmails
                .Where(email => email.EnteredBy != null && email.EnteredBy.Trim().ToLower() == user.Trim().ToLower() && email.Id == id)
                .FirstOrDefault();

            if (templateForUserWithId == null)
                return NotFound();

            return templateForUserWithId;
        }
        

        // PUT: api/MassMailerEmailTemplates/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamCannedEmails(int id, CamCannedEmail camCannedEmails)
        {
            if (id != camCannedEmails.Id)
            {
                return BadRequest();
            }

            _context.Entry(camCannedEmails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CamCannedEmailsExists(id))
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

        // POST: api/MassMailerEmailTemplates
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<CamCannedEmail>> PostCamCannedEmails(CamCannedEmail camCannedEmails)
        {
            _context.CamCannedEmails.Add(camCannedEmails);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CamCannedEmailsExists(camCannedEmails.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCamCannedEmails", new { id = camCannedEmails.Id }, camCannedEmails);
        }

        // DELETE: api/MassMailerEmailTemplates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CamCannedEmail>> DeleteCamCannedEmails(int id)
        {
            var camCannedEmails = await _context.CamCannedEmails.FindAsync(id);
            if (camCannedEmails == null)
            {
                return NotFound();
            }

            _context.CamCannedEmails.Remove(camCannedEmails);
            await _context.SaveChangesAsync();

            return camCannedEmails;
        }

        private bool CamCannedEmailsExists(int id)
        {
            return _context.CamCannedEmails.Any(e => e.Id == id);
        }
    }
}

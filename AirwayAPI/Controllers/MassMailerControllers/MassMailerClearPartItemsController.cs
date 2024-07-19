using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerClearPartItemsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerClearPartItemsController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/MassMailerClearPartItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MassMailerPartItem>> GetMassMailerPartItem(int id)
        {
            var massMailerPartItems = await _context.EquipmentRequests
                .Where(er => er.MassMailing == true
                    && er.MassMailDate.HasValue // Check if MassMailDate is not null
                    && EF.Functions.DateDiffDay(er.MassMailDate.Value, DateTime.Now) == 0
                    && er.MassMailSentBy == id)
                .ToListAsync();


            for (var i = 0; i < massMailerPartItems.Count; i++)
            {
                massMailerPartItems[i].MassMailing = false;
            }
            
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}

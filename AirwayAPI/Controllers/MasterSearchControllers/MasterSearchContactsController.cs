using AirwayAPI.Data;
using AirwayAPI.Models.MasterSearchModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MasterSearch
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterSearchContactsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MasterSearchContactsController(eHelpDeskContext context)
        {
            this._context = context;
        }

        // GET: api/MasterSearchContacts
        [HttpGet]
        public async Task<ActionResult<MasterSearchContact[]>> getMasterSearchContacts([FromQuery] string searchValue, [FromQuery] bool active)
        {
            var query = _context.CamContacts.Where(cc => cc.Contact.Trim().ToLower().Contains(searchValue.Trim().ToLower()));
            if (active)
                query = query.Where(cc => cc.ActiveStatus == 1);
            
            var contacts = await query.Select(cc => new MasterSearchContact{
                    Id = cc.Id,
                    Contact = cc.Contact,
                    Company = cc.Company,
                    State = cc.State,
                    PhoneMain = cc.PhoneMain,
                    ActiveStatus = cc.ActiveStatus == 1
                }).ToListAsync();
                                
            return Ok(contacts);
        }
    }
}
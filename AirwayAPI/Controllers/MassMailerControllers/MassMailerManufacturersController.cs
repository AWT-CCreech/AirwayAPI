using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerManufacturersController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerManufacturersController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/MassMailerManufacturers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetMfgList()
        {
            return await _context.CamFieldsLists
                .Where(cfl => (cfl.ListName != null && cfl.ListName.Trim().Equals("massmailer", StringComparison.CurrentCultureIgnoreCase)) &&
                              (cfl.FieldName != null && cfl.FieldName.Trim().Equals("mfg", StringComparison.CurrentCultureIgnoreCase)))
                .OrderBy(cfl => cfl.FieldValue)
                .Select(cfl => cfl.FieldValue ?? string.Empty) // Replace null FieldValue with empty string
                .ToListAsync();
        }
    }
}

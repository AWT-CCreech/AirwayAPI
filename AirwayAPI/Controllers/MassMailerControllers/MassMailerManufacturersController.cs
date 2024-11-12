using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.MassMailerControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MassMailerManufacturersController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        // GET: api/MassMailerManufacturers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetMfgList()
        {
            var massMailerListName = "massmailer";
            var mfgFieldName = "mfg";

            return await _context.CamFieldsLists
                .Where(cfl =>
                    (cfl.ListName != null && cfl.ListName.Trim().ToLower() == massMailerListName) &&
                    (cfl.FieldName != null && cfl.FieldName.Trim().ToLower() == mfgFieldName))
                .OrderBy(cfl => cfl.FieldValue)
                .Select(cfl => cfl.FieldValue ?? string.Empty) // Replace null FieldValue with empty string
                .ToListAsync();
        }
    }
}

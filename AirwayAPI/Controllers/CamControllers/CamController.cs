using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace AirwayAPI.Controllers.CamControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CamController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public CamController(eHelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet("ContactSearch")]
        public async Task<IActionResult> ContactSearch(
            [FromQuery] string username,
            [FromQuery] string searchText,
            [FromQuery] string searchBy = "Contact",
            [FromQuery] bool activeOnly = true,
            [FromQuery] string orderBy = "Contact",
            [FromQuery] string companyId = "AIR")
        {
            // Adjust the CompanyID based on the username logic from the original ASP page
            if (username.Equals("bhale", StringComparison.OrdinalIgnoreCase) ||
                username.Equals("jherbst", StringComparison.OrdinalIgnoreCase) ||
                username.Equals("avillers", StringComparison.OrdinalIgnoreCase))
            {
                companyId = "SOL";
            }


            // Sanitize and adjust parameters
            searchText = searchText.Trim();
            orderBy = string.IsNullOrEmpty(orderBy) ? searchBy : orderBy;

            // Build the query
            var query = _context.CamContacts
                .Where(c => c.CompanyId == companyId && EF.Functions.Like(EF.Property<string>(c, searchBy), $"%{searchText}%"));

            if (activeOnly)
                query = query.Where(c => c.ActiveStatus == 1);

            query = query.OrderBy(c => EF.Property<object>(c, orderBy));

            // Execute query and get results
            var results = await query.ToListAsync();

            return Ok(results);
        }

        [HttpGet("SearchFields")]
        public async Task<IActionResult> GetSearchFields()
        {
            var searchFields = await _context.CamFieldsLists
                .Where(f => f.FieldName == "Search")
                .OrderBy(f => f.FieldValue)
                .Select(f => f.FieldValue)
                .ToListAsync();

            return Ok(searchFields);
        }

        [HttpGet("AdvancedSearchFields")]
        public async Task<IActionResult> GetAdvancedSearchFields()
        {
            var fields = await _context.CamFieldsLists
                .Where(f => f.FieldName == "AdvSearch")
                .OrderBy(f => f.FieldValue)
                .Select(f => new { f.FieldValue, f.FieldValue2, f.FieldValue4 })
                .ToListAsync();

            return Ok(fields);
        }

        [HttpGet("ContactTypes")]
        public async Task<IActionResult> GetContactTypes()
        {
            var contactTypes = await _context.CamFieldsLists
                .Where(f => f.FieldName == "ContactType")
                .OrderBy(f => f.FieldValue)
                .Select(f => f.FieldValue)
                .ToListAsync();

            return Ok(contactTypes);
        }
    }
}

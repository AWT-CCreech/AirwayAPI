using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AirwayAPI.Controllers.CamControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CamController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        [HttpGet("ContactSearch")]
        public async Task<IActionResult> ContactSearch(
            [FromQuery] string username,
            [FromQuery] string searchText,
            [FromQuery] string searchBy = "Contact",
            [FromQuery] bool activeOnly = true,
            [FromQuery] string orderBy = "Contact",
            [FromQuery] string companyId = "AIR")
        {
            try
            {
                // Adjust the CompanyID based on username
                var loweredUsername = username.ToLower();
                if (loweredUsername == "bhale" || loweredUsername == "jherbst" || loweredUsername == "avillers")
                {
                    companyId = "SOL";
                }

                // Sanitize and validate parameters
                searchText = searchText?.Trim() ?? string.Empty;
                orderBy = string.IsNullOrEmpty(orderBy) ? searchBy : orderBy;

                // Validate 'searchBy' and 'orderBy' fields
                var validFields = new[] { "Contact", "CompanyName", "Email", "PhoneNumber" };
                if (!validFields.Contains(searchBy) || !validFields.Contains(orderBy))
                {
                    return BadRequest("Invalid search or order field.");
                }

                // Build the query
                var query = _context.CamContacts
                    .Where(c => c.CompanyId == companyId &&
                                EF.Functions.Like(EF.Property<string>(c, searchBy), $"%{searchText}%"));

                if (activeOnly)
                {
                    query = query.Where(c => c.ActiveStatus == 1);
                }

                query = query.OrderBy(c => EF.Property<object>(c, orderBy));

                // Execute query and get results
                var results = await query.ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("SearchFields")]
        public async Task<IActionResult> GetSearchFields()
        {
            try
            {
                var searchFields = await _context.CamFieldsLists
                    .Where(f => f.FieldName == "Search")
                    .OrderBy(f => f.FieldValue)
                    .Select(f => f.FieldValue)
                    .ToListAsync();

                return Ok(searchFields);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to retrieve search fields.");
            }
        }

        [HttpGet("AdvancedSearchFields")]
        public async Task<IActionResult> GetAdvancedSearchFields()
        {
            try
            {
                var fields = await _context.CamFieldsLists
                    .Where(f => f.FieldName == "AdvSearch")
                    .OrderBy(f => f.FieldValue)
                    .Select(f => new { f.FieldValue, f.FieldValue2, f.FieldValue4 })
                    .ToListAsync();

                return Ok(fields);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to retrieve advanced search fields.");
            }
        }

        [HttpGet("ContactTypes")]
        public async Task<IActionResult> GetContactTypes()
        {
            try
            {
                var contactTypes = await _context.CamFieldsLists
                    .Where(f => f.FieldName == "ContactType")
                    .OrderBy(f => f.FieldValue)
                    .Select(f => f.FieldValue)
                    .ToListAsync();

                return Ok(contactTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to retrieve contact types.");
            }
        }
    }
}

using AirwayAPI.Data;
using AirwayAPI.Models.EquipmentRequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.EventControllers
{
    //[Authorize] // Uncomment this once EventSearch page is live
    [ApiController]
    [Route("api/[controller]")]
    public class EventSearchController : ControllerBase
    {
        // Dependency Injection: `eHelpDeskContext` is injected here so we can query the database
        private readonly eHelpDeskContext _context;

        // Constructor Injection: The DI system provides `eHelpDeskContext` when this controller is created
        public EventSearchController(eHelpDeskContext context)
        {
            _context = context; // Assign context so it's available for all methods in this controller
        }

        /// <summary>
        /// Searches for equipment requests using a variety of criteria, including date range, status, 
        /// sales representative by ID, company, contact, and project name.
        /// </summary>
        /// <param name="criteria">Contains search filters like dates, company, and rep ID.</param>
        /// <returns>A list of equipment requests that match the specified criteria, including sales rep username.</returns>
        [HttpGet("EquipmentRequestSearch")]
        public async Task<IActionResult> EquipmentRequestSearch([FromQuery] EquipmentRequestSearchCriteria criteria)
        {
            // Initial LINQ query setup: joining multiple tables with DefaultIfEmpty for left joins
            var query = from re in _context.RequestEvents
                        join er in _context.EquipmentRequests on re.EventId equals er.EventId into equipmentRequests
                        from er in equipmentRequests.DefaultIfEmpty() // Left join with EquipmentRequest
                        join c in _context.CamContacts on re.ContactId equals c.Id into contacts
                        from c in contacts.DefaultIfEmpty() // Left join with CamContacts
                        join u in _context.Users on re.EventOwner equals u.Id into users
                        from u in users.DefaultIfEmpty() // Left join with Users to get rep information
                        select new
                        {
                            re.EventId,
                            re.EntryDate,
                            re.ProjectName,
                            re.SoldOrLost,
                            c.Company,
                            c.Contact,
                            SalesRep = u.Uname,
                            er.Status
                        };

            // Date Range Filter: Apply only if both `FromDate` and `ToDate` are provided in `criteria`
            if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
            {
                query = query.Where(re => re.EntryDate >= criteria.FromDate && re.EntryDate <= criteria.ToDate);
            }

            // Filter by Company: Perform a partial match if `Company` is provided
            if (!string.IsNullOrEmpty(criteria.Company))
            {
                query = query.Where(re => EF.Functions.Like(re.Company, $"%{criteria.Company}%"));
            }

            // Filter by Contact: Similar to Company filter, applies a partial match
            if (!string.IsNullOrEmpty(criteria.Contact))
            {
                query = query.Where(re => EF.Functions.Like(re.Contact, $"%{criteria.Contact}%"));
            }

            // Project Name Filter: Checks if the `ProjectName` field contains the specified text
            if (!string.IsNullOrEmpty(criteria.ProjectName))
            {
                query = query.Where(re => EF.Functions.Like(re.ProjectName, $"%{criteria.ProjectName}%"));
            }

            // SalesRep Filter: Checks if the `SalesRep` field contains the selected SalesRep
            if (!string.IsNullOrEmpty(criteria.SalesRep))
            {
                query = query.Where(re => re.SalesRep == criteria.SalesRep);
            }

            // Status Filter: Applies if `Status` is specified, ignores "All" to avoid filtering
            if (!string.IsNullOrEmpty(criteria.Status) && criteria.Status != "All")
            {
                query = query.Where(re => re.Status == criteria.Status);
            }

            // Default to sorting by Entry Date in descending order (newest entries appear first), all other sorting is handled in frontend
            query = query.OrderByDescending(re => re.EntryDate);

            // Execute the query and convert the results to a list asynchronously
            var results = await query.ToListAsync();

            // Return the results wrapped in an OK response
            return Ok(results);
        }
    }
}

using AirwayAPI.Data;
using AirwayAPI.Models.EquipmentRequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AirwayAPI.Controllers.EventControllers
{
    //[Authorize] // Uncomment this once EventSearch page is live
    [ApiController]
    [Route("api/[controller]")]
    public class EventSearchController : ControllerBase
    {
        // Dependency Injection: `eHelpDeskContext` is injected here so we can query the database
        private readonly eHelpDeskContext _context;
        private readonly ILogger<EventSearchController> _logger; // Inject logger

        // Constructor Injection: The DI system provides `eHelpDeskContext` and `ILogger` when this controller is created
        public EventSearchController(eHelpDeskContext context, ILogger<EventSearchController> logger)
        {
            _context = context; // Assign context so it's available for all methods in this controller
            _logger = logger; // Assign logger
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
            try
            {
                // Log the incoming search criteria
                _logger.LogInformation("Received EquipmentRequestSearch with criteria: {@Criteria}", criteria);

                // Initial LINQ query setup: joining multiple tables
                var query = from re in _context.RequestEvents
                            join er in _context.EquipmentRequests on re.EventId equals er.EventId
                            join c in _context.CamContacts on re.ContactId equals c.Id
                            join u in _context.Users on re.EventOwner equals u.Id
                            select new
                            {
                                re.EventId,
                                re.EntryDate,
                                re.ProjectName,
                                c.Company,
                                c.Contact,
                                SalesRep = u.Uname, // Change to u.Lname if needed
                                re.SoldOrLost
                            };

                // Apply Date Range Filter: Only if both `FromDate` and `ToDate` are provided
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    // To include the entire end day, add one day to ToDate and use < instead of <=
                    var adjustedToDate = criteria.ToDate.Value.Date.AddDays(1);
                    query = query.Where(re => re.EntryDate >= criteria.FromDate.Value.Date && re.EntryDate < adjustedToDate);
                    _logger.LogInformation("Applied Date Range Filter: From {FromDate} To {ToDate}", criteria.FromDate.Value, criteria.ToDate.Value);
                }

                // Apply Company Name Filter (Partial Match)
                if (!string.IsNullOrEmpty(criteria.Company))
                {
                    query = query.Where(re => EF.Functions.Like(re.Company, $"%{criteria.Company}%"));
                    _logger.LogInformation("Applied Company Filter: {Company}", criteria.Company);
                }

                // Apply Contact Name Filter (Partial Match)
                if (!string.IsNullOrEmpty(criteria.Contact))
                {
                    query = query.Where(re => EF.Functions.Like(re.Contact, $"%{criteria.Contact}%"));
                    _logger.LogInformation("Applied Contact Filter: {Contact}", criteria.Contact);
                }

                // Apply Project Name Filter (Partial Match)
                if (!string.IsNullOrEmpty(criteria.ProjectName))
                {
                    query = query.Where(re => EF.Functions.Like(re.ProjectName, $"%{criteria.ProjectName}%"));
                    _logger.LogInformation("Applied ProjectName Filter: {ProjectName}", criteria.ProjectName);
                }

                // Apply SalesRep Filter (Exact Match)
                if (!string.IsNullOrEmpty(criteria.SalesRep) && criteria.SalesRep != "All")
                {
                    // Assuming SalesRep is mapped to u.Uname
                    query = query.Where(re => re.SalesRep == criteria.SalesRep);
                    _logger.LogInformation("Applied SalesRep Filter: {SalesRep}", criteria.SalesRep);
                }

                // Apply Status Filter (Exact Match)
                if (!string.IsNullOrEmpty(criteria.Status) && criteria.Status != "All")
                {
                    query = query.Where(re => re.SoldOrLost == criteria.Status);
                    _logger.LogInformation("Applied Status Filter: {Status}", criteria.Status);
                }

                // Apply DISTINCT to eliminate duplicate records
                query = query.Distinct();
                _logger.LogInformation("Applied DISTINCT to eliminate duplicate records.");

                // Sorting: Order by EventID descending
                //query = query.OrderByDescending(re => re.EventId);
                //_logger.LogInformation("Applied Sorting: Order by EventID descending");

                // Capture the SQL query before execution
                var sqlQuery = query.ToQueryString();
                _logger.LogInformation("Generated SQL Query: {SqlQuery}", sqlQuery);

                // Measure query execution time
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Execute the query and convert the results to a list asynchronously
                var results = await query.ToListAsync();

                stopwatch.Stop();
                _logger.LogInformation("Query executed in {ElapsedMilliseconds} ms. Number of records retrieved: {Count}", stopwatch.ElapsedMilliseconds, results.Count);

                // Return the results wrapped in an OK response
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing EquipmentRequestSearch.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}

using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController(eHelpDeskContext context, ILogger<SalesOrderWorkbenchController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<SalesOrderWorkbenchController> _logger = logger;

        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData([FromQuery] int? salesRepId, [FromQuery] string? billToCompany, [FromQuery] int? eventId)
        {
            try
            {
                var query = from so in _context.QtSalesOrders
                            join u in _context.Users on so.AccountMgr equals u.Id into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where so.RwsalesOrderNum == "0" && so.Draft == false
                            select new
                            {
                                so.SaleId,
                                so.EventId,
                                so.QuoteId,
                                so.Version,
                                so.BillToCompanyName,
                                so.SaleTotal,
                                so.SaleDate,
                                so.AccountMgr, // for filtering
                                SalesRep = u != null ? u.Uname : "N/A"
                            };

                if (eventId.HasValue && eventId.Value != 0)
                {
                    query = query.Where(q => q.EventId == eventId.Value);
                }
                else
                {
                    if (salesRepId.HasValue && salesRepId.Value != 0)
                        query = query.Where(q => q.AccountMgr == salesRepId.Value);

                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));
                }

                var result = await query
                    .OrderBy(q => q.EventId)
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetEventLevelData: {ex.Message}", ex);
                return StatusCode(500, "Error fetching Event Level Data");
            }
        }

        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData([FromQuery] int? salesRepId, [FromQuery] string? billToCompany, [FromQuery] int? eventId)
        {
            try
            {
                var query = from d in _context.QtSalesOrderDetails
                            join so in _context.QtSalesOrders on d.SaleId equals so.SaleId
                            join u in _context.Users on so.AccountMgr equals u.Id into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where d.Soflag == true
                            select new
                            {
                                d.Id,
                                d.RequestId,
                                d.QtySold,
                                d.UnitMeasure,
                                d.PartNum,
                                d.PartDesc,
                                d.UnitPrice,
                                d.ExtendedPrice,
                                so.RwsalesOrderNum,
                                so.EventId,
                                so.BillToCompanyName,
                                so.AccountMgr, // for filtering
                                SalesRep = u != null ? u.Uname : "N/A"
                            };

                if (eventId.HasValue && eventId.Value != 0)
                {
                    query = query.Where(q => q.EventId == eventId.Value);
                }
                else
                {
                    if (salesRepId.HasValue && salesRepId.Value != 0)
                        query = query.Where(q => q.AccountMgr == salesRepId.Value);

                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));
                }

                var result = await query
                    .OrderBy(q => q.RequestId)
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetDetailLevelData: {ex.Message}", ex);
                return StatusCode(500, "Error fetching Detail Level Data");
            }
        }
    }
}

using AirwayAPI.Data;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController(
        eHelpDeskContext context,
        ILogger<SalesOrderWorkbenchController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<SalesOrderWorkbenchController> _logger = logger;

        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData(int? salesRepId, string? billToCompany, int? eventId)
        {
            try
            {
                var query = from so in _context.QtSalesOrders
                            join mgr in _context.Users on so.AccountMgr equals mgr.Id into mgrJoin
                            from mgr in mgrJoin.DefaultIfEmpty()
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
                                SalesRep = mgr.Uname
                            };

                if (eventId.HasValue && eventId != 0)
                    query = query.Where(q => q.EventId == eventId);
                else
                {
                    if (salesRepId.HasValue && salesRepId != 0)
                        query = query.Where(q => q.SaleId == salesRepId);
                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => EF.Functions.Like(q.BillToCompanyName, $"{billToCompany}%"));
                }

                var result = await query.OrderBy(q => q.EventId).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetEventLevelData: {ex.Message}");
                return StatusCode(500, "Error fetching Event Level Data");
            }
        }

        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData(int? salesRepId, string? billToCompany, int? eventId)
        {
            try
            {
                var query = from detail in _context.QtSalesOrderDetails
                            join order in _context.QtSalesOrders on detail.SaleId equals order.SaleId
                            join mgr in _context.Users on order.AccountMgr equals mgr.Id into mgrJoin
                            from mgr in mgrJoin.DefaultIfEmpty()
                            where detail.Soflag == true
                            select new
                            {
                                detail.Id,
                                detail.RequestId,
                                detail.QtySold,
                                detail.UnitMeasure,
                                detail.PartNum,
                                detail.PartDesc,
                                detail.UnitPrice,
                                detail.ExtendedPrice,
                                SalesRep = mgr.Uname,
                                order.RwsalesOrderNum,
                                order.EventId,
                                order.AccountMgr,
                                order.BillToCompanyName
                            };

                if (eventId.HasValue && eventId != 0)
                    query = query.Where(q => q.EventId == eventId);
                else
                {
                    if (salesRepId.HasValue && salesRepId != 0)
                        query = query.Where(q => q.AccountMgr == salesRepId);
                    if (!string.IsNullOrWhiteSpace(billToCompany))
                        query = query.Where(q => EF.Functions.Like(q.BillToCompanyName, $"{billToCompany}%"));
                }

                var result = await query.OrderBy(q => q.RequestId).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDetailLevelData: {ex.Message}");
                return StatusCode(500, "Error fetching Detail Level Data");
            }
        }
    }
}

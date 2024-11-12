using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.DataControllers.Inventory
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScanHistoryController(eHelpDeskContext context, ILogger<ScanHistoryController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<ScanHistoryController> _logger = logger;

        [HttpGet("GetDropShipParts")]
        public async Task<IActionResult> GetDropShipParts([FromQuery] string? poNo = null, [FromQuery] string? soNo = null)
        {
            DateTime firstDayOfYear = new(DateTime.Now.Year, 1, 1);
            try
            {
                var query = from er in _context.EquipmentRequests.AsNoTracking()
                            where er.DropShipment == true && !string.IsNullOrEmpty(er.PartNum)
                            join sh in _context.ScanHistories.AsNoTracking()
                            on er.PartNum equals sh.PartNo
                            where !string.IsNullOrEmpty(sh.SerialNo)
                            && er.EntryDate > firstDayOfYear
                            let SoNo = string.IsNullOrEmpty(sh.SoNo) ? er.SalesOrderNum : sh.SoNo
                            join so in _context.TrkRwSoheaders on SoNo equals so.OrderNum into soGroup
                            from so in soGroup.DefaultIfEmpty()
                            orderby er.PartNum, sh.SerialNo
                            select new
                            {
                                PoNo = string.IsNullOrEmpty(sh.PoNo) ? "" : sh.PoNo,
                                SoNo,
                                PartNumber = er.PartNum,
                                SerialNumber = sh.SerialNo,
                                CustomerName = so != null ? so.CustomerName : null,
                                RequiredDate = so != null ? so.RequiredDate : (DateTime?)null
                            };

                // Apply filters if provided
                if (!string.IsNullOrEmpty(poNo))
                {
                    query = query.Where(x => x.PoNo == poNo);
                }

                if (!string.IsNullOrEmpty(soNo))
                {
                    query = query.Where(x => x.SoNo == soNo);
                }

                var dropShipParts = await query.ToListAsync();

                return Ok(dropShipParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drop ship parts");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

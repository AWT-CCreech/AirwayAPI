using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.DropShipControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DropShipInfoController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public DropShipInfoController(eHelpDeskContext context)
        {
            this._context = context;
        }

        [HttpGet("{poNum}")]
        public async Task<ActionResult<object>> GetDropShipInfo(string poNum)
        {
            var SOs = await _context.QtSalesOrders
                .Where(so => so.RwsalesOrderNum != null && so.RwsalesOrderNum.Contains(poNum))
                .ToArrayAsync();

            int salesRepId = 0;
            for (var i = 0; i < SOs.Length; ++i)
            {
                var salesOrderNum = SOs[i].RwsalesOrderNum;
                if (salesOrderNum == poNum ||
                    (salesOrderNum?.Contains(',') == true && salesOrderNum.Split(',').Contains(poNum)))
                {
                    salesRepId = SOs[i].AccountMgr ?? 0;
                }
            }

            if (salesRepId != 0)
            {
                var salesRep = await _context.Users
                    .Where(u => u.Id == salesRepId)
                    .Select(u => new { u.Email, FullName = u.Fname + " " + u.Lname })
                    .FirstOrDefaultAsync();
                return Ok(salesRep);
            }
            else
            {
                return Ok(null);
            }
        }
    }
}

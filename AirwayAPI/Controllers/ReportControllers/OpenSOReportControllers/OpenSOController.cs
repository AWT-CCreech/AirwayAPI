using Microsoft.AspNetCore.Mvc;
using AirwayAPI.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AirwayAPI.Controllers.OpenSalesOrderControllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenSOController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public OpenSOController(eHelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOpenSalesOrders(
            [FromQuery] string salesTeam = "ALL",
            [FromQuery] string SONum = "",
            [FromQuery] string PartNum = "",
            [FromQuery] string DateFilterType = "OrderDate",
            [FromQuery] DateTime? Date1 = null,
            [FromQuery] DateTime? Date2 = null,
            [FromQuery] string lstRep = "All",
            [FromQuery] string CustPO = "",
            [FromQuery] string PONum = "",
            [FromQuery] string ReqDateStatus = "All",
            [FromQuery] string Customer = "",
            [FromQuery] bool chkExcludeCo = false,
            [FromQuery] bool chkAllHere = false,
            [FromQuery] bool chkPONote = false,
            [FromQuery] int PONoteDate = 7,
            [FromQuery] string OrderBy = "SONum",
            [FromQuery] string AscDesc = "Asc",
            [FromQuery] string Category = "All",
            [FromQuery] string AccountNo = "All",
            [FromQuery] bool chkGroupBySO = false)
        {
            var query = _context.OpenSoreports.AsQueryable();

            if (DateFilterType == "OrderDate" && Date1.HasValue && Date2.HasValue)
            {
                query = query.Where(o => o.OrderDate >= Date1 && o.OrderDate <= Date2);
            }
            else if (DateFilterType == "ExpectedDelivery" && Date1.HasValue && Date2.HasValue)
            {
                query = query.Where(o => o.ExpectedDelivery >= Date1 && o.ExpectedDelivery <= Date2);
            }

            if (!string.IsNullOrEmpty(SONum))
            {
                query = query.Where(o => o.Sonum == SONum);
            }

            if (!string.IsNullOrEmpty(PartNum))
            {
                query = query.Where(o => o.MfgNum != null && o.MfgNum.Contains(PartNum));
            }

            if (!string.IsNullOrEmpty(CustPO))
            {
                query = query.Where(o => o.CustPo != null && o.CustPo.Contains(CustPO));
            }

            if (!string.IsNullOrEmpty(PONum))
            {
                query = query.Where(o => o.Ponum == PONum);
            }

            if (!string.IsNullOrEmpty(Customer))
            {
                var customers = Customer.Split(',').ToList();
                if (chkExcludeCo)
                {
                    query = query.Where(o => o.CustomerName == null || !customers.Contains(o.CustomerName));
                }
                else
                {
                    query = query.Where(o => o.CustomerName != null && customers.Contains(o.CustomerName));
                }
            }

            if (ReqDateStatus == "Late")
            {
                query = query.Where(o => o.RequiredDate < DateTime.Now);
            }

            if (salesTeam != "ALL")
            {
                query = query.Where(o => o.AccountTeam == salesTeam);
            }

            if (lstRep != "All")
            {
                query = query.Where(o => o.SalesRep == lstRep);
            }

            if (chkAllHere)
            {
                query = query.Where(o => o.AllHere == true);
            }

            if (chkPONote)
            {
                query = query.Where(o => o.PonoteDate < DateTime.Now.AddDays(-PONoteDate));
            }

            if (chkGroupBySO)
            {
                query = query
                    .GroupBy(o => o.Sonum ?? string.Empty)
                    .Select(g => g.FirstOrDefault()!)
                    .Where(o => o != null);
            }

            query = query.OrderBy($"{OrderBy} {AscDesc}");

            var result = await query.ToListAsync();

            return Ok(result);
        }
    }
}

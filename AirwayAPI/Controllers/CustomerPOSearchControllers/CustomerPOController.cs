using AirwayAPI.Data;
using AirwayAPI.Models.CustomerPOSearchModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.CustomerPOSearchControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerPOController(eHelpDeskContext eHelpDeskContext, MAS500AppContext mas500Context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = eHelpDeskContext;
        private readonly MAS500AppContext _mas500Context = mas500Context;

        // GET: api/CustomerPO/search?PONum=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string PONum)
        {
            if (string.IsNullOrWhiteSpace(PONum))
            {
                return BadRequest("PONum is required.");
            }

            // Primary query: join tsoSalesOrder and tarCustomer by CustKey
            var orders = await (
                from so in _mas500Context.TsoSalesOrders
                join cust in _mas500Context.TarCustomers on so.CustKey equals cust.CustKey
                where so.CustPono.Contains(PONum)
                select new CustomerPOSearchResult
                {
                    // Mimic the ASP code to extract the last 6 characters from TranNo
                    SoTranNo = so.TranNo.Length >= 6 ? so.TranNo.Substring(so.TranNo.Length - 6) : so.TranNo,
                    CustPoNo = so.CustPono,
                    SoTranAmt = so.TranAmt,
                    SoTranDate = so.TranDate,
                    CustomerName = cust.CustName,
                    CustNum = cust.CustId
                }
            ).ToListAsync();

            // For each order, run a secondary query to get SaleID, QuoteID, and EventID.
            foreach (var order in orders)
            {
                var secondary = await _context.QtSalesOrders
                    .Where(q => EF.Functions.Like(q.RwsalesOrderNum, "%" + order.SoTranNo + "%"))
                    .FirstOrDefaultAsync();

                if (secondary != null)
                {
                    order.EventID = (int)secondary.EventId;
                    order.SaleID = secondary.SaleId;
                    order.QuoteID = (int)secondary.QuoteId;
                }
            }

            if (orders.Count == 0)
            {
                return NotFound("Sorry, we could not locate that PO number.");
            }

            return Ok(orders);
        }
    }
}

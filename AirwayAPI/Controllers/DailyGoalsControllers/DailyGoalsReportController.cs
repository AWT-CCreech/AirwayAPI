using AirwayAPI.Data;
using AirwayAPI.Models.DailyGoalsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AirwayAPI.Controllers.DailyGoalsControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DailyGoalsReportController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly MAS500AppContext _mas500Context;

        public DailyGoalsReportController(eHelpDeskContext context, MAS500AppContext mas500Context)
        {
            _context = context;
            _mas500Context = mas500Context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyGoalsReport([FromQuery] string Months, [FromQuery] string Years)
        {
            // Determine Month and Year (default to current if not provided)
            int nMonths = !string.IsNullOrEmpty(Months) && int.TryParse(Months, out var monthValue) ? monthValue : DateTime.Now.Month;
            int nYears = !string.IsNullOrEmpty(Years) && int.TryParse(Years, out var yearValue) ? yearValue : DateTime.Now.Year;

            // Define start and end dates for the month
            int daysInMonth = DateTime.DaysInMonth(nYears, nMonths);
            DateTime startOfMonth = new DateTime(nYears, nMonths, 1);
            DateTime endOfMonth = new DateTime(nYears, nMonths, daysInMonth);

            // -----------------------------------------------------------------------
            // Execute stored procedure via output parameters
            // -----------------------------------------------------------------------
            var startDateParam = new SqlParameter("@StartDate", startOfMonth);
            var endDateParam = new SqlParameter("@EndDate", endOfMonth);

            // Define output parameters (adjust types as needed)
            var totalUncommittedParam = new SqlParameter
            {
                ParameterName = "@TotalUnCommitted",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };
            var committedShipmentsParam = new SqlParameter
            {
                ParameterName = "@CommittedShipments",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };
            var postedShipmentsParam = new SqlParameter
            {
                ParameterName = "@PostedShipments",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };
            var shipmentsInvoicedNotPostedParam = new SqlParameter
            {
                ParameterName = "@ShipmentsInvoicedNotPosted",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };
            var shipmentsInvoicedAndPostedParam = new SqlParameter
            {
                ParameterName = "@ShipmentsInvoicedAndPosted",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };
            var soBatchTotalParam = new SqlParameter
            {
                ParameterName = "@SoBatchTotal",
                SqlDbType = System.Data.SqlDbType.Money,
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_sel_SOinBatchTotal @StartDate, @EndDate, @TotalUnCommitted OUTPUT, @CommittedShipments OUTPUT, @PostedShipments OUTPUT, @ShipmentsInvoicedNotPosted OUTPUT, @ShipmentsInvoicedAndPosted OUTPUT, @SoBatchTotal OUTPUT",
                startDateParam, endDateParam,
                totalUncommittedParam, committedShipmentsParam, postedShipmentsParam,
                shipmentsInvoicedNotPostedParam, shipmentsInvoicedAndPostedParam, soBatchTotalParam);

            var soBatchTotal = (decimal)soBatchTotalParam.Value;

            // -----------------------------------------------------------------------
            // Initialize aggregates and build daily records
            // -----------------------------------------------------------------------
            var totalSold = 0m;
            var totalShipped = 0m;
            var totalNewInvoiced = 0m;
            var totalBackOrderValueToday = 0m;
            var items = new List<DailyGoalItem>();

            for (int x = 1; x <= daysInMonth; x++)
            {
                DateTime vDate = new DateTime(nYears, nMonths, x);
                // Stop processing if vDate is today's date
                if (nMonths == DateTime.Now.Month && vDate.Day == DateTime.Now.Day)
                    break;

                // Get today's backorder value from trkUnshippedValues
                var backOrderValueToday = await _context.TrkUnshippedValues
                    .Where(r => r.ShipDate == vDate.Date)
                    .Select(r => r.UnshippedValue)
                    .FirstOrDefaultAsync() ?? 0;

                // Determine yesterday's date (adjust for Monday)
                DateTime aDate = (vDate.DayOfWeek == DayOfWeek.Monday) ? vDate.AddDays(-3) : vDate.AddDays(-1);
                var backOrderValueYesterday = await _context.TrkUnshippedValues
                    .Where(r => r.ShipDate == aDate.Date)
                    .Select(r => r.UnshippedValue)
                    .FirstOrDefaultAsync() ?? 0;

                // Calculate daily shipped value from tarInvoice (MAS500_app)
                decimal dailyShipped = await _mas500Context.TarInvoices
                    .Where(t => t.PostDate.Date == vDate.Date && t.CompanyId == "AIR")
                    .SumAsync(t => (decimal?)t.TranAmt) ?? 0;
                totalShipped += dailyShipped;

                // Calculate daily sold value from trkRwSoHeader
                decimal dailySold = await _context.TrkRwSoheaders
                    .Where(h => h.OrderDate == vDate.Date && h.CompanyId == "AIR")
                    .SumAsync(h => h.QuoteTotal) ?? 0;
                if (dailySold < 0)
                    dailySold = 0;
                totalSold += dailySold;

                // Calculate newly invoiced value from tarInvoice (MAS500_app)
                var newlyInvoiced = await _mas500Context.TarInvoices
                    .Where(t => t.PostDate.Date == vDate.Date && t.CreateDate > DateTime.Now.AddDays(-1) && t.CompanyId == "AIR")
                    .SumAsync(t => (decimal?)t.TranAmt) ?? 0;
                totalNewInvoiced += newlyInvoiced;

                if (backOrderValueToday > 0)
                    totalBackOrderValueToday = (decimal)backOrderValueToday;

                var displayBackOrder = backOrderValueToday > 0 ? backOrderValueToday - (double)newlyInvoiced : 0;

                items.Add(new DailyGoalItem
                {
                    Date = vDate,
                    DailySold = dailySold,
                    DailyShipped = dailyShipped,
                    UnshippedBackOrder = (decimal)displayBackOrder
                });
            }

            var totals = new DailyGoalTotals
            {
                TotalSold = totalSold,
                TotalShipped = totalShipped + soBatchTotal,
                TotalBackOrder = totalBackOrderValueToday - totalNewInvoiced,
                SoBatchTotal = soBatchTotal
            };

            var report = new DailyGoalsReport
            {
                Items = items,
                Totals = totals
            };

            return Ok(report);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetDailyGoalsDetail([FromQuery] string DisplayType, [FromQuery] string SearchDate)
        {
            if (string.IsNullOrEmpty(DisplayType) || string.IsNullOrEmpty(SearchDate))
            {
                return BadRequest("Missing DisplayType or SearchDate");
            }

            // Attempt to parse the SearchDate. Adjust the format if needed.
            if (!DateTime.TryParse(SearchDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime searchDate))
            {
                return BadRequest("Invalid date format.");
            }

            List<DailyGoalDetail> details = [];

            if (DisplayType.Equals("Sold", StringComparison.OrdinalIgnoreCase))
            {
                // Query for Sold details from trkRwSoHeader
                details = await _context.TrkRwSoheaders
                    .Where(x => x.OrderDate == searchDate.Date &&
                                x.CompanyId == "AIR" &&
                                x.Type != 3 && x.Type != 2)
                    .OrderBy(x => x.CustomerName)
                    .Select(x => new DailyGoalDetail
                    {
                        OrderNum = x.OrderNum,
                        CustomerName = x.CustomerName,
                        QuoteTotal = (decimal)x.QuoteTotal
                    })
                    .ToListAsync();
            }
            else if (DisplayType.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
            {
                // Query for Shipped details from MAS500_app
                details = await (
                    from inv in _mas500Context.TarInvoices
                    join cust in _mas500Context.TarCustomers on inv.CustKey equals cust.CustKey into custGroup
                    from customer in custGroup.DefaultIfEmpty()
                    where inv.PostDate.Date == searchDate.Date && inv.CompanyId == "AIR"
                    orderby customer.CustName
                    select new DailyGoalDetail
                    {
                        // Mimic the ASP code: extract the last 6 characters for the SoTranNo.
                        OrderNum = inv.TranNo.Length >= 6 ? inv.TranNo.Substring(inv.TranNo.Length - 6) : inv.TranNo,
                        CustomerName = customer != null ? customer.CustName : string.Empty,
                        QuoteTotal = inv.SalesAmt,
                        // Optional: You can extend these queries to calculate costs.
                        InvoiceCost = 0,
                        ConsignCost = 0
                    }
                ).ToListAsync();
            }
            else if (DisplayType.Equals("Unshipped", StringComparison.OrdinalIgnoreCase))
            {
                // Query for Unshipped details using a join between TrkUnshippedByitemno and trkRwSoHeader.
                // This example assumes that you have a DbSet<TrkUnshippedByitemno> in _context.
                details = await (
                    from un in _context.TrkUnshippedByItemNos
                    join so in _context.TrkRwSoheaders on un.SoNo equals so.OrderNum
                    where un.DateRecorded == searchDate.Date &&
                          so.CompanyId == "AIR" &&
                          so.Type != 3 && so.Type != 2
                    group new { un, so } by new { so.OrderNum, so.CustomerName } into g
                    orderby g.Key.OrderNum
                    select new DailyGoalDetail
                    {
                        OrderNum = g.Key.OrderNum,
                        CustomerName = g.Key.CustomerName,
                        QuoteTotal = (decimal)g.Sum(x => x.un.UnshippedValue)
                    }
                ).ToListAsync();
            }
            else
            {
                return BadRequest("Invalid DisplayType");
            }

            return Ok(details);
        }
    }
}

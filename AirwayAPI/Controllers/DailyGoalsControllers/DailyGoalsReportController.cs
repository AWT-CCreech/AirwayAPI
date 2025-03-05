using AirwayAPI.Data;
using AirwayAPI.Models.DailyGoalsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
                    DisplayBackOrder = (decimal)displayBackOrder
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
    }
}

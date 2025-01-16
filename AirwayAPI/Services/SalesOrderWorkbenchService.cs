// SalesOrderWorkbenchService.cs
using AirwayAPI.Data;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class SalesOrderWorkbenchService : ISalesOrderWorkbenchService
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<SalesOrderWorkbenchService> _logger;

        public SalesOrderWorkbenchService(
            eHelpDeskContext context,
            ILogger<SalesOrderWorkbenchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<object>> GetEventLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId)
        {
            // Equivalent to your original query
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
                            so.AccountMgr,
                            SalesRep = u != null ? u.Uname : "N/A"
                        };

            // Conditionals
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

            // Log SQL
            var sql = query.ToQueryString();
            _logger.LogDebug("GetEventLevelData SQL Query: {sql}", sql);

            var result = await query.OrderBy(q => q.EventId).ToListAsync<object>();
            return result;
        }

        public async Task<List<object>> GetDetailLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId)
        {
            var query = from d in _context.QtSalesOrderDetails
                        join so in _context.QtSalesOrders on d.SaleId equals so.SaleId
                        join er in _context.EquipmentRequests on d.RequestId equals er.RequestId
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
                            er.SalesOrderNum,
                            so.EventId,
                            so.BillToCompanyName,
                            so.AccountMgr,
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

            // Log SQL
            var sql = query.ToQueryString();
            _logger.LogDebug("GetDetailLevelData SQL Query: {sql}", sql);

            var result = await query.OrderBy(q => q.RequestId).ToListAsync<object>();
            return result;
        }
    }
}

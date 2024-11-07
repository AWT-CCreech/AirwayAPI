using AirwayAPI.Data;
using AirwayAPI.Models.SalesOrderWorkbenchModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly EmailService _emailService;

        public SalesOrderWorkbenchController(eHelpDeskContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/SalesOrderWorkbench/EventLevelData
        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData(int? salesRepId, string? billToCompany, int? eventId)
        {
            var query = from so in _context.QtSalesOrders
                        join mgr in _context.Users on so.AccountMgr equals mgr.Id into mgrJoin
                        from mgr in mgrJoin.DefaultIfEmpty()
                        where so.RwsalesOrderNum == "0" && so.Draft == false
                        select new
                        {
                            SalesOrder = so,
                            AccountManager = mgr
                        };

            if (salesRepId.HasValue)
                query = query.Where(q => q.AccountManager != null && q.AccountManager.Id == salesRepId.Value);

            if (!string.IsNullOrWhiteSpace(billToCompany))
                query = query.Where(q => !string.IsNullOrEmpty(q.SalesOrder.BillToCompanyName) && EF.Functions.Like(q.SalesOrder.BillToCompanyName, $"{billToCompany}%"));

            if (eventId.HasValue)
                query = query.Where(q => q.SalesOrder.EventId == eventId);

            var salesOrders = await query.OrderBy(q => q.SalesOrder.EventId).ToListAsync();
            return Ok(salesOrders);
        }

        // GET: api/SalesOrderWorkbench/DetailLevelData
        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData(int? salesRepId, string? billToCompany, int? eventId)
        {
            var query = from detail in _context.QtSalesOrderDetails
                        join order in _context.QtSalesOrders on detail.SaleId equals order.SaleId
                        join mgr in _context.Users on order.AccountMgr equals mgr.Id into mgrJoin
                        from mgr in mgrJoin.DefaultIfEmpty()
                        where detail.Soflag == true && order.Draft == false
                        select new
                        {
                            SalesOrderDetail = detail,
                            SalesOrder = order,
                            AccountManager = mgr
                        };

            if (salesRepId.HasValue)
                query = query.Where(q => q.AccountManager != null && q.AccountManager.Id == salesRepId.Value);

            if (!string.IsNullOrWhiteSpace(billToCompany))
                query = query.Where(q => !string.IsNullOrEmpty(q.SalesOrder.BillToCompanyName) && EF.Functions.Like(q.SalesOrder.BillToCompanyName, $"{billToCompany}%"));

            if (eventId.HasValue)
                query = query.Where(q => q.SalesOrder.EventId == eventId);

            var details = await query.OrderBy(q => q.SalesOrderDetail.RequestId).ToListAsync();
            return Ok(details);
        }

        // POST: api/SalesOrderWorkbench/UpdateSalesOrder
        [HttpPost("UpdateSalesOrder")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderUpdateDto request)
        {
            var salesOrder = await _context.QtSalesOrders.FirstOrDefaultAsync(so => so.SaleId == request.SaleId);
            if (salesOrder == null) return NotFound();

            salesOrder.RwsalesOrderNum = request.RWSalesOrderNum;
            salesOrder.DropShipment = request.DropShipment;
            await _context.SaveChangesAsync();

            // Update the corresponding quote
            var quote = await _context.QtQuotes.FirstOrDefaultAsync(q => q.EventId == request.EventId && q.QuoteId == request.QuoteId);
            if (quote != null)
            {
                quote.RwsalesOrderNum = request.RWSalesOrderNum;
                await _context.SaveChangesAsync();
            }

            // Send notification email
            if (!string.IsNullOrEmpty(salesOrder.BillToCompanyName) && salesOrder.BillToCompanyName.Contains("VERIZON"))
            {
                await _emailService.SendEmailAsync(
                    fromEmail: "it_department@airway.com",
                    toEmail: "sbaker@airway.com",
                    subject: request.Subject,
                    htmlBody: request.HtmlBody,
                    userName: request.Username,
                    password: request.Password);
            }

            return Ok();
        }

        // POST: api/SalesOrderWorkbench/UpdateEquipmentRequest
        [HttpPost("UpdateEquipmentRequest")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            var salesOrderDetail = await _context.QtSalesOrderDetails.FirstOrDefaultAsync(d => d.Id == request.Id);
            if (salesOrderDetail == null) return NotFound();

            var requestItem = await _context.EquipmentRequests.FirstOrDefaultAsync(r => r.RequestId == salesOrderDetail.RequestId);
            if (requestItem != null)
            {
                requestItem.Status = "Sold";
                requestItem.SalesOrderNum = request.RWSalesOrderNum;
                requestItem.SalePrice = salesOrderDetail.UnitPrice;
                requestItem.MarkedSoldDate = DateTime.Now;
                requestItem.QtySold += salesOrderDetail.QtySold;

                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}

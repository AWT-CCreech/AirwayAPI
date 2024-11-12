using AirwayAPI.Data;
using AirwayAPI.Models.SalesOrderWorkbenchModels;
using AirwayAPI.Models.ServiceModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers.SalesOrderWorkbenchControllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderWorkbenchController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<SalesOrderWorkbenchController> _logger;

        public SalesOrderWorkbenchController(
            eHelpDeskContext context,
            EmailService emailService,
            ILogger<SalesOrderWorkbenchController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/SalesOrderWorkbench/EventLevelData
        [HttpGet("EventLevelData")]
        public async Task<IActionResult> GetEventLevelData(
            int? salesRepId,
            string? billToCompany,
            int? eventId)
        {
            try
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

                var salesOrders = await query
                    .OrderBy(q => q.SalesOrder.EventId)
                    .ToListAsync();

                return Ok(salesOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching EventLevelData: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching Event Level Data.");
            }
        }

        // GET: api/SalesOrderWorkbench/DetailLevelData
        [HttpGet("DetailLevelData")]
        public async Task<IActionResult> GetDetailLevelData(
            int? salesRepId,
            string? billToCompany,
            int? eventId)
        {
            try
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

                var details = await query
                    .OrderBy(q => q.SalesOrderDetail.RequestId)
                    .ToListAsync();

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching DetailLevelData: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while fetching Detail Level Data.");
            }
        }

        // POST: api/SalesOrderWorkbench/UpdateSalesOrder
        [HttpPost("UpdateSalesOrder")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId);
                if (salesOrder == null)
                    return NotFound("Sales order not found.");

                salesOrder.RwsalesOrderNum = request.RWSalesOrderNum;
                salesOrder.DropShipment = request.DropShipment;
                await _context.SaveChangesAsync();

                // Update the corresponding quote
                var quote = await _context.QtQuotes
                    .FirstOrDefaultAsync(q => q.EventId == request.EventId && q.QuoteId == request.QuoteId);
                if (quote != null)
                {
                    quote.RwsalesOrderNum = request.RWSalesOrderNum;
                    await _context.SaveChangesAsync();
                }

                // Send notification email
                if (!string.IsNullOrEmpty(salesOrder.BillToCompanyName) && salesOrder.BillToCompanyName.Contains("VERIZON"))
                {
                    await SendNotificationEmailAsync(request, "sbaker@airway.com");
                }

                return Ok("Sales order updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating SalesOrder: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the sales order.");
            }
        }

        // POST: api/SalesOrderWorkbench/UpdateEquipmentRequest
        [HttpPost("UpdateEquipmentRequest")]
        public async Task<IActionResult> UpdateEquipmentRequest([FromBody] EquipmentRequestUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var salesOrderDetail = await _context.QtSalesOrderDetails
                    .FirstOrDefaultAsync(d => d.Id == request.Id);
                if (salesOrderDetail == null)
                    return NotFound("Sales order detail not found.");

                var requestItem = await _context.EquipmentRequests
                    .FirstOrDefaultAsync(r => r.RequestId == salesOrderDetail.RequestId);
                if (requestItem == null)
                    return NotFound("Equipment request item not found.");

                requestItem.Status = "Sold";
                requestItem.SalesOrderNum = request.RWSalesOrderNum;
                requestItem.SalePrice = salesOrderDetail.UnitPrice;
                requestItem.MarkedSoldDate = DateTime.Now;
                requestItem.QtySold += salesOrderDetail.QtySold;

                await _context.SaveChangesAsync();

                return Ok("Equipment request updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating EquipmentRequest: {ex.Message}", ex);
                return StatusCode(500, "An error occurred while updating the equipment request.");
            }
        }

        // Helper method to send notification email
        private async Task SendNotificationEmailAsync(SalesOrderUpdateDto request, string toEmail)
        {
            var emailInput = new EmailInput
            {
                FromEmail = "it_department@airway.com",
                ToEmail = toEmail,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                UserName = request.Username,
                Password = request.Password,
            };

            await _emailService.SendEmailAsync(emailInput);
        }
    }
}
using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Models.EmailModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AirwayAPI.Services
{
    public class SalesOrderWorkbenchService(
        eHelpDeskContext context,
        ILogger<SalesOrderWorkbenchService> logger,
        IEmailService emailService,
        IUserService userService,
        IStringService stringService
        ) : ISalesOrderWorkbenchService
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<SalesOrderWorkbenchService> _logger = logger;
        private readonly IEmailService _emailService = emailService;
        private readonly IUserService _userService = userService;
        private readonly IStringService _stringService = stringService;

        #region 1) GET methods (Event-Level & Detail-Level)

        /// <summary>
        /// Get the event-level data based on the given filters
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <param name="billToCompany"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<List<object>> GetEventLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId)
        {
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
                            SalesRep = u != null ? u.Uname : "N/A",
                            so.DropShipment
                        };

            // Filter conditions
            if (eventId.HasValue && eventId.Value != 0)
                query = query.Where(q => q.EventId == eventId.Value);
            if (salesRepId.HasValue && salesRepId.Value != 0)
                query = query.Where(q => q.AccountMgr == salesRepId.Value);
            if (!string.IsNullOrWhiteSpace(billToCompany))
                query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));

            var sql = query.ToQueryString();
            _logger.LogDebug("GetEventLevelData SQL Query: {sql}", sql);

            var result = await query.OrderBy(q => q.EventId).ToListAsync<object>();
            return result;
        }

        /// <summary>
        /// Get the detail-level data based on the given filters
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <param name="billToCompany"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
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
                            so.RwsalesOrderNum,
                            so.EventId,
                            so.BillToCompanyName,
                            so.AccountMgr,
                            SalesRep = u != null ? u.Uname : "N/A",
                            er.DropShipment
                        };

            // Filter conditions
            if (eventId.HasValue && eventId.Value != 0)
                query = query.Where(q => q.EventId == eventId.Value);
            if (salesRepId.HasValue && salesRepId.Value != 0)
                query = query.Where(q => q.AccountMgr == salesRepId.Value);
            if (!string.IsNullOrWhiteSpace(billToCompany))
                query = query.Where(q => q.BillToCompanyName.StartsWith(billToCompany));

            var sql = query.ToQueryString();
            _logger.LogDebug("GetDetailLevelData SQL Query: {sql}", sql);

            var result = await query.OrderBy(q => q.RequestId).ToListAsync<object>();
            return result;
        }

        #endregion

        #region 2) POST methods (UpdateEventLevel & UpdateDetailLevel)

        /// <summary>
        /// Update the SalesOrder record and related entities at the event level
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateEventLevelAsync(SalesOrderUpdateDto request)
        {
            // Start a transaction so that all changes either commit or roll back together
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Update the QtSalesOrders record
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == request.SaleId)
                    ?? throw new Exception($"Sales order with SaleID#{request.SaleId} not found.");

                // For instance, updating RwsalesOrderNum with "ReplaceDelimiters"
                salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
                salesOrder.DropShipment = request.DropShipment;
                salesOrder.EditDate = DateTime.Now;

                _context.QtSalesOrders.Update(salesOrder);
                await _context.SaveChangesAsync();

                // 2) Update the related quote
                await UpdateQuoteAsync(request);

                // 3) Update the detail-level items & associated EquipmentRequests
                var details = await _context.QtSalesOrderDetails
                    .Where(d => d.SaleId == request.SaleId && d.QtySold > 0)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    await ProcessEquipmentRequest(detail, request);
                }

                // 4) (Optional) Send email notifications about the sales order change
                await NotifySalesOrderChanges(request, salesOrder);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateEventLevelAsync Transaction rolled back: {Message}", ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Update the EquipmentRequest record and related entities at the detail level
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateDetailLevelAsync(EquipmentRequestUpdateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1a) Retrieve NEW EventID based on `request.SalesOrderNum`
                var query = _context.EquipmentRequests
                    .Where(er => er.SalesOrderNum == request.SalesOrderNum)
                    .Select(er => er.EventId);

                // 1b) Get the raw SQL for logging/debugging
                var rawSql = query.ToQueryString();
                _logger.LogDebug("SQL Query for retrieving EventID: {rawSql}", rawSql);

                // 1c) Execute the query and retrieve the result
                var checkEventID = await query.FirstOrDefaultAsync()
                    ?? throw new Exception($"EventID not found for SalesOrderNum {request.SalesOrderNum}");

                _logger.LogDebug("EventID found for SalesOrderNum {0}: {1}", request.SalesOrderNum, checkEventID);

                // 2a) Retrieve the QtSalesOrderDetail record based on `request.RequestId`
                var salesOrderDetail = await _context.QtSalesOrderDetails
                    .FirstOrDefaultAsync(so => so.RequestId == request.RequestId)
                    ?? throw new Exception($"Sales Order Detail with RequestID#{request.RequestId} not found.");

                // 2b) [Optional To-Do] Update the Soflag to improve UX by avoiding reliance on overnight scripts
                // salesOrderDetail.Soflag = false;
                // _context.QtSalesOrderDetails.Update(salesOrderDetail);
                // await _context.SaveChangesAsync();

                // 2c) Retrieve the QtSalesOrder record and ensure the EventID is updated
                var salesOrder = await _context.QtSalesOrders
                    .FirstOrDefaultAsync(so => so.SaleId == salesOrderDetail.SaleId)
                    ?? throw new Exception($"Sales Order with SaleID#{salesOrderDetail.SaleId} not found");

                if (salesOrder.EventId != checkEventID)
                    salesOrder.EventId = checkEventID;

                // 2d) Update the RwsalesOrderNum and EditDate fields
                salesOrder.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
                salesOrder.EditDate = DateTime.Now;

                // 2e) Save the updated QtSalesOrder
                _context.QtSalesOrders.Update(salesOrder);
                await _context.SaveChangesAsync();

                // 3a) Retrieve the QtQuote record associated with the SalesOrder's EventID
                var quote = await _context.QtQuotes
                    .Where(qt => qt.RwsalesOrderNum.Length > 0
                                 && qt.Approved == true
                                 && qt.EventId == salesOrder.EventId)
                    .FirstOrDefaultAsync()
                    ?? throw new Exception($"Quote with EventID#{salesOrder.EventId} not found");

                // 3b) Update the EventID and RwsalesOrderNum fields in the quote
                if (quote.EventId != checkEventID)
                    quote.EventId = checkEventID;
                quote.RwsalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);

                // 3c) Save the updated QtQuote record
                _context.QtQuotes.Update(quote);
                await _context.SaveChangesAsync();

                // 4a) Retrieve the EquipmentRequest record based on `request.RequestId`
                var detail = await _context.EquipmentRequests
                    .FirstOrDefaultAsync(d => d.RequestId == request.RequestId)
                    ?? throw new Exception($"EquipmentRequest with RequestId #{request.RequestId} not found.");

                // 4b) Update the EventID, SalesOrderNum, ModifiedBy, and ModifiedDate fields
                if (detail.EventId != checkEventID)
                    detail.EventId = checkEventID;
                detail.SalesOrderNum = _stringService.ReplaceDelimiters(request.SalesOrderNum);
                detail.ModifiedBy = await _userService.GetUserIdAsync(request.Username);
                detail.ModifiedDate = DateTime.Now;

                // 4c) Save the updated EquipmentRequest record
                _context.EquipmentRequests.Update(detail);
                await _context.SaveChangesAsync();

                // 5a) Notify relevant stakeholders via email about the EquipmentRequest changes
                await NotifyEquipmentRequestChanges(request, detail);

                // 5b) Commit the transaction to ensure all updates are saved atomically
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateDetailLevelAsync Transaction rolled back: {Message}", ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region 3) Internal Helper Methods (merged from existing services)

        /// <summary>
        /// Process the EquipmentRequest and update the related entities
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task ProcessEquipmentRequest(QtSalesOrderDetail detail, SalesOrderUpdateDto request)
        {
            _logger.LogInformation(
                "Processing EquipmentRequest for RequestId {RequestId} with DropShipment: {DropShipment}",
                detail.RequestId, request.DropShipment
            );

            var equipmentRequest = await _context.EquipmentRequests
                .FirstOrDefaultAsync(r => r.RequestId == detail.RequestId)
                ?? throw new Exception($"EquipmentRequest with RequestID {detail.RequestId} not found.");

            // Overwrite or append the SalesOrderNum as needed
            equipmentRequest.SalesOrderNum = UpdateSalesOrderNum(equipmentRequest.SalesOrderNum ?? "", request.SalesOrderNum);
            equipmentRequest.Status = "Sold";
            equipmentRequest.SalePrice = detail.UnitPrice;
            equipmentRequest.MarkedSoldDate = DateTime.Now;
            equipmentRequest.QtySold = (equipmentRequest.QtySold ?? 0) + (detail.QtySold ?? 0);
            equipmentRequest.DropShipment = request.DropShipment;
            equipmentRequest.ModifiedBy = await _userService.GetUserIdAsync(request.Username);
            equipmentRequest.ModifiedDate = DateTime.Now;

            // Enough stock to mark as Bought?
            bool canMarkAsBought = await ShouldMarkAsBoughtAsync(equipmentRequest, detail.QtySold ?? 0);
            if (canMarkAsBought)
            {
                equipmentRequest.Bought = true;
                _context.TrkSoldItemHistories.Add(new TrkSoldItemHistory
                {
                    RequestId = equipmentRequest.RequestId,
                    DateMarkedBought = DateTime.Now,
                    Username = request.Username,
                    PageName = "SalesOrderWorkbenchService.ProcessEquipmentRequest"
                });
            }
            else
            {
                equipmentRequest.Bought = false;
            }

            _context.EquipmentRequests.Update(equipmentRequest);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if the EquipmentRequest should be marked as Bought
        /// </summary>
        /// <param name="request"></param>
        /// <param name="qtySold"></param>
        /// <returns></returns>
        private async Task<bool> ShouldMarkAsBoughtAsync(EquipmentRequest request, int qtySold)
        {
            // This basically replicates “NeedToBuy <= 0 => Bought=1, else => Bought=0”
            var inventory = await _context.TrkRwImItems
                .FirstOrDefaultAsync(i => i.ItemNum == request.PartNum || i.AltPartNum == request.PartNum);

            int QtyOnHand = inventory?.QtyOnHand ?? 0;
            int QtyInPick = inventory?.QtyInPick ?? 0;

            int Adjustments = await _context.TrkAdjustments
                .Where(a => a.ItemNum == request.PartNum && a.EntryDate == DateTime.Today)
                .SumAsync(a => a.QtyAdjustment) ?? 0;

            int QtyBought = await _context.RequestPos
                .Where(r => r.RequestId == request.RequestId)
                .SumAsync(r => r.QtyBought) ?? 0;

            int QtySoldToday = await _context.EquipmentRequests
                .Where(e => e.PartNum == request.PartNum &&
                            e.Status == "Sold" &&
                            e.MarkedSoldDate.HasValue &&
                            e.MarkedSoldDate.Value.Date == DateTime.Today)
                .SumAsync(e => e.QtySold) ?? 0;

            int QtyAvailToSell = QtyOnHand + Adjustments - QtyInPick - QtySoldToday;

            _logger.LogInformation("QtyOnHand: {0}, QtyInPick: {1}, Adjustments: {2}, QtyBought: {3}, QtySoldToday: {4}, QtyAvailToSell: {5}",
                QtyOnHand, QtyInPick, Adjustments, QtyBought, QtySoldToday, QtyAvailToSell);

            // if (qtySold - QtyAvailToSell - QtyBought) <= 0 => can mark as bought
            return (qtySold - QtyAvailToSell - QtyBought) <= 0;
        }

        /// <summary>
        /// Update the related quote record
        /// </summary>
        /// <param name="salesOrderUpdate"></param>
        /// <returns></returns>
        private async Task UpdateQuoteAsync(SalesOrderUpdateDto salesOrderUpdate)
        {
            var relatedQuote = await _context.QtQuotes
                .FirstOrDefaultAsync(q => q.QuoteId == salesOrderUpdate.QuoteId &&
                                          q.EventId == salesOrderUpdate.EventId);

            if (relatedQuote == null)
            {
                _logger.LogWarning("No related quote found for QuoteID {0} and EventID {1}",
                    salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
                return;
            }

            // Example: replace semicolons in SalesOrderNum
            relatedQuote.RwsalesOrderNum = salesOrderUpdate.SalesOrderNum.Replace(";", ",");

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated related quote for QuoteID {0} & EventID {1}",
                salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
        }

        /// <summary>
        /// Notify the sales order changes via email
        /// </summary>
        /// <param name="request"></param>
        /// <param name="salesOrder"></param>
        /// <returns></returns>
        private async Task NotifySalesOrderChanges(SalesOrderUpdateDto request, QtSalesOrder salesOrder)
        {
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);
            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{salesOrder.RwsalesOrderNum} has been updated." },
                { "%%NAME%%", senderInfo.FullName },
                { "%%EMAIL%%", senderInfo.Email },
                { "%%JOBTITLE%%", senderInfo.JobTitle },
                { "%%DIRECT%%", senderInfo.DirectPhone },
                { "%%MOBILE%%", senderInfo.MobilePhone }
            };

            var emailInput = new EmailInputBase
            {
                FromEmail = senderInfo.Email,
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"Sales Order {salesOrder.RwsalesOrderNum} Updated",
                Body = "%%EMAILBODY%%",
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            await _emailService.SendEmailAsync(emailInput);
        }

        /// <summary>
        /// Notify the equipment request changes via email
        /// </summary>
        /// <param name="request"></param>
        /// <param name="equipmentRequest"></param>
        /// <returns></returns>
        private async Task NotifyEquipmentRequestChanges(EquipmentRequestUpdateDto request, EquipmentRequest equipmentRequest)
        {
            var senderInfo = await _emailService.GetSenderInfoAsync(request.Username);
            var placeholders = new Dictionary<string, string>
            {
                { "%%EMAILBODY%%", $"SO#{equipmentRequest.SalesOrderNum} has been updated." },
                { "%%NAME%%", senderInfo.FullName },
                { "%%EMAIL%%", senderInfo.Email },
                { "%%JOBTITLE%%", senderInfo.JobTitle },
                { "%%DIRECT%%", senderInfo.DirectPhone },
                { "%%MOBILE%%", senderInfo.MobilePhone }
            };

            var emailInput = new EmailInputBase
            {
                FromEmail = senderInfo.Email,
                ToEmails = new List<string> { "ccreech@airway.com" },
                Subject = $"Sales Order {equipmentRequest.SalesOrderNum} Updated",
                Body = "%%EMAILBODY%%",
                Placeholders = placeholders,
                UserName = request.Username,
                Password = request.Password
            };

            await _emailService.SendEmailAsync(emailInput);
        }

        /// <summary>
        /// Update the SalesOrderNum field based on the existing and new values.
        /// Can be reformatted for logging or other purposes.
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="newNum"></param>
        /// <returns></returns>
        private static string UpdateSalesOrderNum(string existing, string newNum)
        {
            // To simply overwrite, just return newNum.
            // To append, you can do something like:
            //  return string.IsNullOrWhiteSpace(existing) ? newNum : $"{existing},{newNum}";
            return newNum;
        }
        #endregion
    }
}

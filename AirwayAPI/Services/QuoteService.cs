using AirwayAPI.Data;
using AirwayAPI.Models.DTOs;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<QuoteService> _logger;

        public QuoteService(eHelpDeskContext context, ILogger<QuoteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateQuoteAsync(SalesOrderUpdateDto salesOrderUpdate)
        {
            try
            {
                // Fetch the related quote
                var relatedQuote = await _context.QtQuotes
                    .FirstOrDefaultAsync(q => q.QuoteId == salesOrderUpdate.QuoteId &&
                                              q.EventId == salesOrderUpdate.EventId);

                if (relatedQuote == null)
                {
                    _logger.LogWarning("No related quote found for Quote ID: {QuoteId} and Event ID: {EventId}",
                        salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
                    return;
                }

                // Update the quote's Sales Order Number
                relatedQuote.RwsalesOrderNum = salesOrderUpdate.SalesOrderNum.Replace(";", ",");

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated related quote for Quote ID: {QuoteId} & Event ID: {EventId}",
                    salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating related quote: {Message}", ex.Message);
                throw;
            }
        }
    }
}

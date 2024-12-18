using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

        /// <summary>
        /// Updates the related quote in the qtQuote table with sales order and drop shipment details.
        /// </summary>
        /// <param name="salesOrderUpdate">The sales order update details.</param>
        /// <returns></returns>
        public async Task UpdateQuoteAsync(SalesOrderUpdateDto salesOrderUpdate)
        {
            try
            {
                // Fetch the related quote
                var relatedQuote = await _context.QtQuotes
                    .FirstOrDefaultAsync(q => q.QuoteId == salesOrderUpdate.QuoteId && q.EventId == salesOrderUpdate.EventId);

                if (relatedQuote == null)
                {
                    _logger.LogWarning("No related quote found for Quote ID: {QuoteId} and Event ID: {EventId}", salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
                    return;
                }

                // Update the quote's Sales Order Number and Drop Shipment status
                relatedQuote.RwsalesOrderNum = salesOrderUpdate.RWSalesOrderNum.Replace(";", ",");

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated related quote for Quote ID: {QuoteId} and Event ID: {EventId}", salesOrderUpdate.QuoteId, salesOrderUpdate.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating related quote: {Message}", ex.Message);
                throw;
            }
        }
    }
}

using AirwayAPI.Models;
using AirwayAPI.Models.FreightSheetModels;

namespace AirwayAPI.Services.Interfaces
{
    /// <summary>
    /// Defines all operations needed to support Freight_Sheet logic (Save / Update / AddRow / Get).
    /// </summary>
    public interface ILogisticsService
    {
        /// <summary>
        /// Inserts a brand‐new FreightQuote (header + one default FreightSO line) exactly as frmAction="Save" did.
        /// Returns the newly‐inserted FreightQuote.Id.
        /// </summary>
        Task<int> CreateFreightQuoteAsync(FreightQuoteDto dto, string currentUserName);

        /// <summary>
        /// Updates an existing FreightQuote header and upserts each FreightSO line.  Mirrors frmAction="Update".
        /// </summary>
        Task UpdateFreightQuoteAsync(FreightQuoteDto dto, string currentUserName);

        /// <summary>
        /// Inserts one new FreightSO row (all-zero values) for an existing FreightQuote.Id.
        /// Mirrors frmAction="AddRow". Returns the newly‐created FreightSO.Id.
        /// </summary>
        Task<int> AddFreightSoLineAsync(int freightQuoteId, string currentUserName);

        /// <summary>
        /// Retrieves the FreightQuote header by Id.  (Used when loading an existing sheet.)
        /// </summary>
        Task<FreightQuote> GetFreightQuoteByIdAsync(int freightQuoteId);

        /// <summary>
        /// Retrieves all FreightSO lines associated with a given FreightQuote.Id.
        /// </summary>
        Task<List<FreightSo>> GetFreightSoLinesByQuoteIdAsync(int freightQuoteId);
    }
}

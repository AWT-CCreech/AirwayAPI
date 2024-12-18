using AirwayAPI.Models.DTOs;

namespace AirwayAPI.Services
{
    public interface IQuoteService
    {
        Task UpdateQuoteAsync(SalesOrderUpdateDto request);
    }
}

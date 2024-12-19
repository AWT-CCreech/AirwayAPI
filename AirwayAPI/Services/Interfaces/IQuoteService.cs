using AirwayAPI.Models.DTOs;

namespace AirwayAPI.Services.Interfaces
{
    public interface IQuoteService
    {
        Task UpdateQuoteAsync(SalesOrderUpdateDto request);
    }
}

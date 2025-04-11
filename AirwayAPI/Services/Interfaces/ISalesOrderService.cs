using AirwayAPI.Models.SalesOrderWorkbenchModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface ISalesOrderService
    {
        Task<List<object>> GetEventLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId);
        Task<List<object>> GetDetailLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId);

        Task UpdateEventLevelAsync(EventLevelUpdateDto request);
        Task UpdateDetailLevelAsync(DetailLevelUpdateDto request);
    }
}

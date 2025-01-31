using AirwayAPI.Models.DTOs;

namespace AirwayAPI.Services.Interfaces
{
    public interface ISalesOrderWorkbenchService
    {
        Task<List<object>> GetEventLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId);
        Task<List<object>> GetDetailLevelDataAsync(int? salesRepId, string? billToCompany, int? eventId);

        Task UpdateEventLevelAsync(EventLevelUpdateDto request);
        Task UpdateDetailLevelAsync(DetailLevelUpdateDto request);
    }
}

using AirwayAPI.Models.PODeliveryLogModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface IPurchasingService
    {
        Task<List<PODeliveryLogSearchResult>> GetPODeliveryLogsAsync(PODeliveryLogQueryParameters p);
        Task<List<string>> GetVendorsAsync(PODeliveryLogQueryParameters p);
    }
}
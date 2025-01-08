using AirwayAPI.Models.DTOs;

namespace AirwayAPI.Services.Interfaces
{
    public interface ISalesOrderService
    {
        Task UpdateSalesOrderAsync(SalesOrderUpdateDto request);
        Task UpdateEquipmentRequestAsync(EquipmentRequestUpdateDto request);
    }
}

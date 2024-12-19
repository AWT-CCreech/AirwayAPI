using AirwayAPI.Models.DTOs;
using System.Threading.Tasks;

namespace AirwayAPI.Services.Interfaces
{
    public interface ISalesOrderService
    {
        Task UpdateSalesOrderAsync(SalesOrderUpdateDto request);
    }
}

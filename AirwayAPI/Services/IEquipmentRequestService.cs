using AirwayAPI.Models.DTOs;
using AirwayAPI.Models;
using System.Threading.Tasks;

namespace AirwayAPI.Services
{
    public interface IEquipmentRequestService
    {
        Task ProcessEquipmentRequest(QtSalesOrderDetail detail, SalesOrderUpdateDto request);
        Task<QtSalesOrderDetail> GetSalesOrderDetailByIdAsync(int id);
    }
}

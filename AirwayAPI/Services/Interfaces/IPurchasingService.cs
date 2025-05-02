using AirwayAPI.Models.GenericDtos;
using AirwayAPI.Models.PODeliveryLogModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface IPurchasingService
    {
        Task<List<PODeliveryLogSearchResult>> GetPODeliveryLogsAsync(PODeliveryLogQueryParameters p);
        Task<List<string>> GetVendorsAsync(PODeliveryLogQueryParameters p);
        Task<PODetailUpdateDto> GetPODetailByIdAsync(int id);
        Task UpdatePODetailAsync(int id, PODetailUpdateDto updateDto);
        Task AddNoteAsync(int id, NoteDto noteDto);
    }
}
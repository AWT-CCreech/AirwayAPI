using AirwayAPI.Models;
using AirwayAPI.Models.ScanHistoryModels;

namespace AirwayAPI.Services.Interfaces
{
    public interface IScanService
    {
        Task<IEnumerable<ScanHistory>> SearchScanHistoryAsync(SearchScansDto searchDto);
        Task<int> DeleteScansAsync(IEnumerable<int> selectedIds);
        Task<int> UpdateScansAsync(IEnumerable<UpdateScanDto> updateDtos);
        Task<int> AddTestLabScansAsync(IEnumerable<int> selectedIds);
        Task<int> CopyScansAsync(CopyScansDto copyRequest);
    }
}

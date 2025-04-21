using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.ScanHistoryModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class ScanService(eHelpDeskContext context, ILogger<ScanService> logger) : IScanService
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<ScanService> _logger = logger;

        public async Task<IEnumerable<ScanHistory>> SearchScanHistoryAsync(SearchScansDto dto)
        {
            try
            {
                _logger.LogInformation("SearchScanHistoryAsync called with DTO: {@DTO}", dto);

                var query = _context.ScanHistories.AsQueryable();

                // Date range
                _logger.LogInformation("Filtering by date range: {Start} - {End}", dto.ScanDateRangeStart, dto.ScanDateRangeEnd);
                query = query.Where(s =>
                    s.ScanDate >= dto.ScanDateRangeStart &&
                    s.ScanDate <= dto.ScanDateRangeEnd);
                _logger.LogInformation("Count after date filtering: {Count}", await query.CountAsync());

                // PartNo
                if (!string.IsNullOrWhiteSpace(dto.PartNo))
                {
                    _logger.LogInformation("Filtering by PartNo: {PartNo}", dto.PartNo);
                    query = query.Where(s => s.PartNo != null && s.PartNo.Contains(dto.PartNo));
                    _logger.LogInformation("Count after PartNo filtering: {Count}", await query.CountAsync());
                }

                // SerialNo + SNField
                if (!string.IsNullOrWhiteSpace(dto.SerialNo))
                {
                    _logger.LogInformation("Filtering by SerialNo ({SNField}): {SerialNo}", dto.SNField, dto.SerialNo);
                    query = dto.SNField switch
                    {
                        "" => query.Where(s =>
                            (s.SerialNo != null && s.SerialNo.Contains(dto.SerialNo)) ||
                            (s.SerialNoB != null && s.SerialNoB.Contains(dto.SerialNo)) ||
                            (s.HeciCode != null && s.HeciCode.Contains(dto.SerialNo))),
                        "HeciCode" => query.Where(s => s.HeciCode != null && s.HeciCode.Contains(dto.SerialNo)),
                        "SerialNo" => query.Where(s => s.SerialNo != null && s.SerialNo.Contains(dto.SerialNo)),
                        "SerialNoB" => query.Where(s => s.SerialNoB != null && s.SerialNoB.Contains(dto.SerialNo)),
                        _ => query
                    };
                    _logger.LogInformation("Count after SerialNo filtering: {Count}", await query.CountAsync());
                }

                // MNSCo
                if (!string.IsNullOrWhiteSpace(dto.MNSCo))
                {
                    _logger.LogInformation("Filtering by MNSCo: {MNSCo}", dto.MNSCo);
                    query = query.Where(s => s.MnsCompany == dto.MNSCo);
                    _logger.LogInformation("Count after MNSCo filtering: {Count}", await query.CountAsync());
                }

                // ScanUser
                if (!string.IsNullOrWhiteSpace(dto.ScanUser))
                {
                    _logger.LogInformation("Filtering by ScanUser: {ScanUser}", dto.ScanUser);
                    query = query.Where(s => s.UserName == dto.ScanUser);
                    _logger.LogInformation("Count after ScanUser filtering: {Count}", await query.CountAsync());
                }

                // OrderType
                if (!string.IsNullOrWhiteSpace(dto.OrderType))
                {
                    _logger.LogInformation("Filtering by OrderType: {OrderType}", dto.OrderType);
                    query = query.Where(s => s.OrderType == dto.OrderType);
                    _logger.LogInformation("Count after OrderType filtering: {Count}", await query.CountAsync());
                }

                // Single OrderNum search (includes RTV/C)
                if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                {
                    _logger.LogInformation("Filtering by OrderNum '{OrderNum}' under OrderType '{OrderType}'",
                        dto.OrderNum, dto.OrderType);

                    query = dto.OrderType switch
                    {
                        "SO" => query.Where(s => s.SoNo != null && s.SoNo.Contains(dto.OrderNum)),
                        "PO" => query.Where(s => s.PoNo != null && s.PoNo.Contains(dto.OrderNum)),
                        "RMA" => query.Where(s => s.Rmano != null && s.Rmano.Contains(dto.OrderNum)),
                        "RTV/C" when int.TryParse(dto.OrderNum, out var rtvId)
                               => query.Where(s => s.Rtvid == rtvId),
                        "" or null
                               => query.Where(s =>
                                    (s.SoNo != null && s.SoNo.Contains(dto.OrderNum)) ||
                                    (s.PoNo != null && s.PoNo.Contains(dto.OrderNum)) ||
                                    (s.Rmano != null && s.Rmano.Contains(dto.OrderNum)) ||
                                    EF.Functions.Like(s.Rtvid.ToString(), $"%{dto.OrderNum}%")
                                  ),
                        _ => query
                    };

                    _logger.LogInformation("Count after OrderNum filtering: {Count}", await query.CountAsync());
                }

                // Order & limit
                query = query.OrderByDescending(s => s.RowId)
                             .Take(dto.Limit);

                _logger.LogInformation("Generated SQL query: {Sql}", query.ToQueryString());
                var results = await query.ToListAsync();
                _logger.LogInformation("Returning {Count} records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching scan history: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<int> DeleteScansAsync(IEnumerable<int> selectedIds)
        {
            try
            {
                _logger.LogInformation("DeleteScansAsync called with IDs: {IDs}", selectedIds);
                var histories = await _context.ScanHistories
                    .Where(s => selectedIds.Contains(s.RowId))
                    .ToListAsync();
                _logger.LogInformation("Records to delete: {Count}", histories.Count);

                _context.ScanHistories.RemoveRange(histories);
                int deleted = await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} records", deleted);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting scan histories: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<int> UpdateScansAsync(IEnumerable<UpdateScanDto> updateDtos)
        {
            int updateCount = 0;
            _logger.LogInformation("UpdateScansAsync called with DTOs: {@DTOs}", updateDtos);

            foreach (var dto in updateDtos)
            {
                try
                {
                    var scan = await _context.ScanHistories.FirstOrDefaultAsync(s => s.RowId == dto.RowId);
                    if (scan != null)
                    {
                        _logger.LogInformation("Updating scan with RowId {RowId}", dto.RowId);
                        if (dto.ScanDate.HasValue)
                            scan.ScanDate = dto.ScanDate.Value;
                        if (!string.IsNullOrWhiteSpace(dto.UserName))
                            scan.UserName = dto.UserName;
                        if (!string.IsNullOrWhiteSpace(dto.OrderType))
                        {
                            scan.OrderType = dto.OrderType;
                            // Update the appropriate order number based on OrderType.
                            switch (dto.OrderType)
                            {
                                case "SO":
                                    if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                                        scan.SoNo = dto.OrderNum;
                                    break;
                                case "PO":
                                    if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                                        scan.PoNo = dto.OrderNum;
                                    break;
                                case "RMA":
                                    if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                                        scan.Rmano = dto.OrderNum;
                                    break;
                                case "RTV/C":
                                    if (int.TryParse(dto.OrderNum, out int rtvId))
                                        scan.Rtvid = rtvId;
                                    break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(dto.PartNo))
                            scan.PartNo = dto.PartNo;
                        if (!string.IsNullOrWhiteSpace(dto.SerialNo))
                            scan.SerialNo = dto.SerialNo;
                        if (!string.IsNullOrWhiteSpace(dto.HeciCode))
                            scan.HeciCode = dto.HeciCode;

                        updateCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Scan with RowId {RowId} not found", dto.RowId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error updating scan with RowId {RowId}: {ErrorMessage}", dto.RowId, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Total updated records: {Count}", updateCount);
            return updateCount;
        }

        public async Task<int> AddTestLabScansAsync(IEnumerable<int> selectedIds)
        {
            int insertCount = 0;
            _logger.LogInformation("AddTestLabScansAsync called with IDs: {IDs}", selectedIds);

            foreach (var id in selectedIds)
            {
                try
                {
                    var scan = await _context.ScanHistories.FirstOrDefaultAsync(s => s.RowId == id);
                    if (scan != null)
                    {
                        _logger.LogInformation("Adding scan with RowId {RowId} to test lab", id);
                        var testLabRecord = new ScanTestLab
                        {
                            ScanHistId = scan.RowId,
                            ScanDate = DateTime.Now,
                            UserName = scan.UserName,
                            ScannerId = scan.ScannerId,
                            PartNo = scan.PartNo,
                            PartNo2 = scan.PartNo2,
                            PartNoClean = scan.PartNoClean,
                            SerialNo = scan.SerialNo,
                            SerialNoB = scan.SerialNoB,
                            HeciCode = scan.HeciCode,
                            Tag = 0,
                            Status = "Pending",
                            OrderType = scan.OrderType,
                            OrderNo = 0,
                            TestResult = string.Empty,
                            Notes = string.Empty,
                            EmailShipping = false,
                            EmailReceiving = false,
                            EmailPurchasing = false,
                            EmailSales = false,
                            CreatedOn = DateTime.Now,
                            CreatedBy = scan.UserName,
                            RedTagAction = string.Empty,
                            RedTagStatus = string.Empty,
                            EditBy = null,
                            EditDate = null,
                            Fn = false
                        };

                        await _context.ScanTestLabs.AddAsync(testLabRecord);
                        insertCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Scan with RowId {RowId} not found for test lab addition", id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error adding scan with RowId {RowId} to test lab: {ErrorMessage}", id, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Total added to test lab: {Count}", insertCount);
            return insertCount;
        }

        public async Task<int> CopyScansAsync(CopyScansDto copyRequest)
        {
            int insertCount = 0;
            _logger.LogInformation("CopyScansAsync called with request: {@Request}", copyRequest);

            string orderDirection;
            string xField, yField;

            if (copyRequest.ToOrderType == "SO")
            {
                xField = "PoNo";
                yField = "SoNo";
                orderDirection = "OUT";
            }
            else
            {
                xField = "SoNo";
                yField = "PoNo";
                orderDirection = "IN";
            }

            foreach (var id in copyRequest.SelectedIDs)
            {
                try
                {
                    var original = await _context.ScanHistories.FirstOrDefaultAsync(s => s.RowId == id);
                    if (original != null)
                    {
                        _logger.LogInformation("Copying scan with RowId {RowId}", id);
                        var copyRecord = new ScanHistory
                        {
                            ScanDate = DateTime.Now,
                            ScannerId = original.ScannerId,
                            MnsCompany = copyRequest.ToCompany,
                            Direction = orderDirection,
                            OrderType = copyRequest.ToOrderType,
                            UserName = copyRequest.RequestedBy,
                            PostId = 0,
                            SoNo = copyRequest.ToOrderType == "SO" ? copyRequest.ToOrderNum : string.Empty,
                            PoNo = copyRequest.ToOrderType == "PO" ? copyRequest.ToOrderNum : string.Empty,
                            Rmano = string.Empty,
                            Rtvid = 0,
                            PartNo = original.PartNo,
                            SerialNo = original.SerialNo,
                            SerialNoB = original.SerialNoB,
                            HeciCode = original.HeciCode,
                            BinLocation = original.BinLocation,
                            TrkEventId = 0,
                            Notes = $"Copied from {copyRequest.FromCompany} {copyRequest.FromOrderType}#{copyRequest.FromOrderNum} for {copyRequest.RequestedBy}"
                        };

                        await _context.ScanHistories.AddAsync(copyRecord);
                        insertCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Scan with RowId {RowId} not found for copying", id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error copying scan with RowId {RowId}: {ErrorMessage}", id, ex.Message);
                }
            }

            if ((copyRequest.FromCompany != "AirWay" && copyRequest.ToCompany == "AirWay") ||
                (copyRequest.FromCompany != "AirWay" && copyRequest.ToCompany != "AirWay"))
            {
                var spSql = $"EXEC dbo.scan_Add_MNS_Transaction @OrderType = '{copyRequest.ToOrderType}', @OrderNum = '{copyRequest.ToOrderNum}'";
                _logger.LogInformation("Executing stored procedure: {SpSql}", spSql);
                await _context.Database.ExecuteSqlRawAsync(spSql);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Total records copied: {Count}", insertCount);
            return insertCount;
        }
    }
}

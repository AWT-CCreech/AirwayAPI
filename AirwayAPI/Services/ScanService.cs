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
                _logger.LogInformation("SearchScanHistoryAsync called with {@DTO}", dto);

                var query = _context.ScanHistories.AsQueryable();

                // Date range
                query = query.Where(s =>
                    s.ScanDate >= dto.ScanDateRangeStart &&
                    s.ScanDate <= dto.ScanDateRangeEnd);

                // PartNo
                if (!string.IsNullOrWhiteSpace(dto.PartNo))
                    query = query.Where(s =>
                        s.PartNo != null &&
                        s.PartNo.Contains(dto.PartNo));

                // SerialNo + SNField
                if (!string.IsNullOrWhiteSpace(dto.SerialNo))
                {
                    _logger.LogInformation("Filtering SerialNo using field '{Field}' for value '{Value}'",
                        dto.SNField, dto.SerialNo);

                    switch (dto.SNField)
                    {
                        // blank SNField → search all three
                        case "":
                            query = query.Where(s =>
                                (s.SerialNo != null && s.SerialNo.Contains(dto.SerialNo)) ||
                                (s.SerialNoB != null && s.SerialNoB.Contains(dto.SerialNo)) ||
                                (s.HeciCode != null && s.HeciCode.Contains(dto.SerialNo)));
                            break;
                        case "SerialNo":
                            query = query.Where(s =>
                                s.SerialNo != null &&
                                s.SerialNo.Contains(dto.SerialNo));
                            break;
                        case "SerialNoB":
                            query = query.Where(s =>
                                s.SerialNoB != null &&
                                s.SerialNoB.Contains(dto.SerialNo));
                            break;
                        case "HeciCode":
                            query = query.Where(s =>
                                s.HeciCode != null &&
                                s.HeciCode.Contains(dto.SerialNo));
                            break;
                        default:
                            // unknown SNField → no filter
                            break;
                    }
                }

                // MNSCo
                if (!string.IsNullOrWhiteSpace(dto.MNSCo))
                    query = query.Where(s => s.MnsCompany == dto.MNSCo);

                // ScanUser
                if (!string.IsNullOrWhiteSpace(dto.ScanUser))
                    query = query.Where(s => s.UserName == dto.ScanUser);

                // OrderType
                if (!string.IsNullOrWhiteSpace(dto.OrderType))
                    query = query.Where(s => s.OrderType == dto.OrderType);

                // OrderNum
                if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                {
                    switch (dto.OrderType)
                    {
                        case "SO":
                            query = query.Where(s => s.SoNo != null && s.SoNo.Contains(dto.OrderNum));
                            break;
                        case "PO":
                            query = query.Where(s => s.PoNo != null && s.PoNo.Contains(dto.OrderNum));
                            break;
                        case "RMA":
                            query = query.Where(s => s.Rmano != null && s.Rmano.Contains(dto.OrderNum));
                            break;
                        case "RTV/C":
                            if (int.TryParse(dto.OrderNum, out var rtvId))
                                query = query.Where(s => s.Rtvid == rtvId);
                            break;
                        default:
                            query = query.Where(s =>
                                (s.SoNo != null && s.SoNo.Contains(dto.OrderNum)) ||
                                (s.PoNo != null && s.PoNo.Contains(dto.OrderNum)) ||
                                (s.Rmano != null && s.Rmano.Contains(dto.OrderNum)) ||
                                EF.Functions.Like(s.Rtvid.ToString(), $"%{dto.OrderNum}%"));
                            break;
                    }
                }

                // Final ordering & limit
                var results = await query
                    .OrderByDescending(s => s.RowId)
                    .Take(dto.Limit)
                    .ToListAsync();

                _logger.LogInformation("SearchScanHistoryAsync returning {Count} records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchScanHistoryAsync");
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

                _context.ScanHistories.RemoveRange(histories);
                var deleted = await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} scan histories", deleted);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteScansAsync");
                throw;
            }
        }

        public async Task<int> UpdateScansAsync(IEnumerable<UpdateScanDto> updateDtos)
        {
            var toApply = updateDtos
                .Where(dto =>
                    dto.ScanDate.HasValue ||
                    !string.IsNullOrWhiteSpace(dto.UserName) ||
                    !string.IsNullOrWhiteSpace(dto.OrderType) ||
                    !string.IsNullOrWhiteSpace(dto.OrderNum) ||
                    !string.IsNullOrWhiteSpace(dto.PartNo) ||
                    !string.IsNullOrWhiteSpace(dto.SerialNo) ||
                    !string.IsNullOrWhiteSpace(dto.HeciCode))
                .ToList();

            if (toApply.Count == 0)
            {
                _logger.LogInformation("UpdateScansAsync: no meaningful updates, skipping");
                return 0;
            }

            _logger.LogInformation("UpdateScansAsync called with {Count} DTOs", toApply.Count);
            var updatedCount = 0;

            foreach (var dto in toApply)
            {
                try
                {
                    var scan = await _context.ScanHistories
                        .FirstOrDefaultAsync(s => s.RowId == dto.RowId);
                    if (scan == null)
                    {
                        _logger.LogWarning("RowId {RowId} not found", dto.RowId);
                        continue;
                    }

                    // 1) Apply OrderType if present
                    if (!string.IsNullOrWhiteSpace(dto.OrderType))
                    {
                        scan.OrderType = dto.OrderType;
                        _logger.LogInformation("Row {RowId}: set OrderType = {Type}", dto.RowId, dto.OrderType);
                    }

                    // 2) Apply OrderNum if present, based on scan.OrderType
                    if (!string.IsNullOrWhiteSpace(dto.OrderNum))
                    {
                        switch (scan.OrderType)
                        {
                            case "SO":
                                scan.SoNo = dto.OrderNum;
                                break;
                            case "PO":
                                scan.PoNo = dto.OrderNum;
                                break;
                            case "RMA":
                                scan.Rmano = dto.OrderNum;
                                break;
                            case "RTV/C":
                                if (int.TryParse(dto.OrderNum, out var id))
                                    scan.Rtvid = id;
                                break;
                        }
                        _logger.LogInformation("Row {RowId}: set OrderNum = {Num} on type {Type}",
                            dto.RowId, dto.OrderNum, scan.OrderType);
                    }

                    // 3) The others exactly as before:
                    if (dto.ScanDate.HasValue)
                        scan.ScanDate = dto.ScanDate.Value;
                    if (!string.IsNullOrWhiteSpace(dto.UserName))
                        scan.UserName = dto.UserName;
                    if (!string.IsNullOrWhiteSpace(dto.PartNo))
                        scan.PartNo = dto.PartNo;
                    if (!string.IsNullOrWhiteSpace(dto.SerialNo))
                        scan.SerialNo = dto.SerialNo;
                    if (!string.IsNullOrWhiteSpace(dto.HeciCode))
                        scan.HeciCode = dto.HeciCode;

                    updatedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating RowId {RowId}", dto.RowId);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateScansAsync persisted {Count} records", updatedCount);
            return updatedCount;
        }

        public async Task<int> AddTestLabScansAsync(IEnumerable<int> selectedIds)
        {
            var insertCount = 0;
            _logger.LogInformation("AddTestLabScansAsync called with IDs: {IDs}", selectedIds);

            foreach (var id in selectedIds)
            {
                try
                {
                    var scan = await _context.ScanHistories
                        .FirstOrDefaultAsync(s => s.RowId == id);

                    if (scan == null)
                    {
                        _logger.LogWarning("ScanRowId {RowId} not found for test lab", id);
                        continue;
                    }

                    var testLabRecord = new ScanTestLab
                    {
                        ScanHistId = scan.RowId,
                        ScanDate = DateTime.UtcNow,
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
                        CreatedOn = DateTime.UtcNow,
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding RowId {RowId} to test lab", id);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("AddTestLabScansAsync added {Count} records", insertCount);
            return insertCount;
        }

        public async Task<int> CopyScansAsync(CopyScansDto copyRequest)
        {
            var insertCount = 0;
            _logger.LogInformation("CopyScansAsync called with {@Request}", copyRequest);

            var (xField, yField, orderDirection) = copyRequest.ToOrderType switch
            {
                "SO" => ("PoNo", "SoNo", "OUT"),
                _ => ("SoNo", "PoNo", "IN")
            };

            foreach (var id in copyRequest.SelectedIDs)
            {
                try
                {
                    var original = await _context.ScanHistories
                        .FirstOrDefaultAsync(s => s.RowId == id);

                    if (original == null)
                    {
                        _logger.LogWarning("ScanRowId {RowId} not found for copy", id);
                        continue;
                    }

                    var copyRecord = new ScanHistory
                    {
                        ScanDate = DateTime.UtcNow,
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
                        Notes = $"Copied from {copyRequest.FromCompany} {copyRequest.FromOrderType}#{copyRequest.FromOrderNum}"
                    };

                    await _context.ScanHistories.AddAsync(copyRecord);
                    insertCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error copying RowId {RowId}", id);
                }
            }

            // Optionally run stored procedure
            if ((copyRequest.FromCompany != "AirWay") &&
                (copyRequest.ToCompany == "AirWay" || copyRequest.ToCompany != "AirWay"))
            {
                var spSql = $"EXEC dbo.scan_Add_MNS_Transaction @OrderType='{copyRequest.ToOrderType}', @OrderNum='{copyRequest.ToOrderNum}'";
                _logger.LogInformation("Executing SP: {Sql}", spSql);
                await _context.Database.ExecuteSqlRawAsync(spSql);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("CopyScansAsync inserted {Count} records", insertCount);
            return insertCount;
        }
    }
}
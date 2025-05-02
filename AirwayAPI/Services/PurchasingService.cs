using AirwayAPI.Data;
using AirwayAPI.Models.PODeliveryLogModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class PurchasingService : IPurchasingService
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<PurchasingService> _logger;

        public PurchasingService(eHelpDeskContext context, ILogger<PurchasingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PODeliveryLogSearchResult>> GetPODeliveryLogsAsync(PODeliveryLogQueryParameters p)
        {
            if (p.YearRange == 0) p.YearRange = DateTime.Now.Year;
            var dateStart = new DateTime(p.YearRange, 1, 1);
            var dateEnd = new DateTime(p.YearRange, 12, 31);
            var companyAll = string.Equals(p.CompanyID, "all", StringComparison.OrdinalIgnoreCase);
            var companyParam = p.CompanyID ?? "";

            _logger.LogInformation(
                "Retrieving PODeliveryLogs for Year={Year}, CompanyID={CompanyID}",
                p.YearRange, p.CompanyID
            );

            // Base query, with null‑safe company filter
            var query = _context.TrkPologs
                .AsNoTracking()
                .Where(log =>
                    log.Deleted != true &&
                    log.IssueDate >= dateStart &&
                    log.IssueDate <= dateEnd &&
                    (companyAll || (log.CompanyId ?? "") == companyParam)
                )
                .Select(log => new PODeliveryLogSearchResult
                {
                    Id = log.Id,
                    Ponum = log.Ponum ?? "",
                    IssueDate = log.IssueDate,
                    ItemNum = log.ItemNum ?? "",
                    QtyOrdered = log.QtyOrdered,
                    QtyReceived = log.QtyReceived,
                    ReceiverNum = log.ReceiverNum,
                    NotesExist = _context.TrkPonotes.Any(n => n.Ponum.ToString() == log.Ponum),
                    NoteEditDate = _context.TrkPonotes
                                          .Where(n => n.Ponum.ToString() == log.Ponum)
                                          .OrderByDescending(n => n.EntryDate)
                                          .Select(n => n.EntryDate.HasValue
                                              ? n.EntryDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                              : null)
                                          .FirstOrDefault(),
                    PORequiredDate = log.RequiredDate,
                    DateDelivered = log.DateDelivered,
                    EditDate = log.EditDate,
                    ExpectedDelivery = log.ExpectedDelivery,
                    Sonum = log.SalesOrderNum ?? "",
                    IssuedBy = log.IssuedBy ?? "",
                    VendorName = _context.TrkRwPoheaders
                                            .Where(h => h.Ponum == log.Ponum)
                                            .OrderByDescending(h => h.Ponum)
                                            .Select(h => h.VendorNum)
                                            .Distinct()
                                            .Join(_context.TrkRwvendors,
                                                  vn => vn,
                                                  v => v.VendorNum,
                                                  (_, v) => v.VendorName)
                                            .FirstOrDefault() ?? "",
                    ItemClassId = _context.TrkRwImItems
                                            .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                                            .OrderByDescending(i => i.ItemNum)
                                            .Select(i => i.ItemClassId)
                                            .FirstOrDefault(),
                    AltPartNum = _context.TrkRwImItems
                                            .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                                            .OrderByDescending(i => i.ItemNum)
                                            .Select(i => i.AltPartNum)
                                            .FirstOrDefault() ?? "",
                    Postatus = _context.TrkRwPoheaders
                                            .Where(h => h.Ponum == log.Ponum)
                                            .OrderByDescending(h => h.Ponum)
                                            .Select(h => h.Postatus)
                                            .FirstOrDefault(),
                    CompanyId = log.CompanyId ?? "",
                    ContactId = log.ContactId,
                    SalesRep = log.SalesRep ?? "",
                    CustomerName = _context.TrkRwSoheaders
                                            .Where(s => s.OrderNum == log.SalesOrderNum)
                                            .OrderByDescending(s => s.OrderNum)
                                            .Select(s => s.CustomerName)
                                            .FirstOrDefault() ?? "",
                    SORequiredDate = _context.TrkRwSoheaders
                                            .Where(s => s.OrderNum == log.SalesOrderNum)
                                            .OrderByDescending(s => s.OrderNum)
                                            .Select(s => s.RequiredDate)
                                            .FirstOrDefault(),
                    IsDropShipment = _context.QtSalesOrders
                                            .Where(q => q.RwsalesOrderNum == log.SalesOrderNum)
                                            .OrderByDescending(q => q.SaleId)
                                            .Select(q => q.DropShipment)
                                            .FirstOrDefault() == true
                });

            // apply optional filters
            if (!string.IsNullOrEmpty(p.PONum))
                query = query.Where(r => r.Ponum.Contains(p.PONum));
            if (!string.IsNullOrEmpty(p.SONum))
                query = query.Where(r => r.Sonum.Contains(p.SONum));
            if (!string.IsNullOrEmpty(p.Vendor))
                query = query.Where(r => r.VendorName.Contains(p.Vendor));
            if (!string.IsNullOrEmpty(p.PartNum))
                query = query.Where(r => r.ItemNum.Contains(p.PartNum));
            if (!string.IsNullOrEmpty(p.xSalesRep)
                && !string.Equals(p.xSalesRep, "all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.SalesRep == p.xSalesRep);
            }

            // POStatus
            switch (p.POStatus.ToLowerInvariant())
            {
                case "not complete":
                    query = query.Where(r => r.QtyOrdered > r.QtyReceived && r.Postatus == 1);
                    break;
                case "complete":
                    query = query.Where(r => r.QtyOrdered <= r.QtyReceived);
                    break;
                case "late":
                    query = query.Where(r =>
                        r.QtyOrdered > r.QtyReceived &&
                        r.Postatus == 1 &&
                        (
                            (r.ExpectedDelivery.HasValue && DateTime.Now > r.ExpectedDelivery.Value && r.PORequiredDate.HasValue && DateTime.Now > r.PORequiredDate.Value) ||
                            (r.PORequiredDate.HasValue && DateTime.Now > r.PORequiredDate.Value && !r.ExpectedDelivery.HasValue)
                        )
                    );
                    break;
                case "due w/n 2 days":
                    query = query.Where(r =>
                        r.QtyOrdered > r.QtyReceived &&
                        r.Postatus == 1 &&
                        (
                            (r.ExpectedDelivery.HasValue && r.ExpectedDelivery.Value >= DateTime.Now && r.ExpectedDelivery.Value <= DateTime.Now.AddDays(2)) ||
                            (r.PORequiredDate.HasValue && r.PORequiredDate.Value >= DateTime.Now && r.PORequiredDate.Value <= DateTime.Now.AddDays(2) && !r.ExpectedDelivery.HasValue)
                        )
                    );
                    break;
            }

            // IssuedBy
            if (!string.IsNullOrEmpty(p.IssuedBy)
                && !string.Equals(p.IssuedBy, "all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.IssuedBy == p.IssuedBy);
            }

            // HasNotes
            if (string.Equals(p.HasNotes, "yes", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.NotesExist);
            else if (string.Equals(p.HasNotes, "no", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => !r.NotesExist);

            // EquipType
            if (string.Equals(p.EquipType, "anc", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.ItemClassId == 23);
            else if (string.Equals(p.EquipType, "fne", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.ItemClassId == 24);

            var results = await query.ToListAsync();
            _logger.LogInformation("Retrieved {Count} PODeliveryLogs after filtering", results.Count);
            return results;
        }

        public async Task<List<string>> GetVendorsAsync(PODeliveryLogQueryParameters p)
        {
            if (p.YearRange == 0) p.YearRange = DateTime.Now.Year;
            var dateStart = new DateTime(p.YearRange, 1, 1);
            var dateEnd = new DateTime(p.YearRange, 12, 31);
            var filterCompany = !string.IsNullOrEmpty(p.CompanyID)
                              && !string.Equals(p.CompanyID, "all", StringComparison.OrdinalIgnoreCase);
            var companyParam = p.CompanyID ?? "";

            _logger.LogInformation(
                "Retrieving Vendors for Year={Year}, CompanyID={CompanyID}",
                p.YearRange, p.CompanyID
            );

            // Join Pologs → Poheaders → Rwvendors
            var vendorsQuery = _context.TrkPologs
                .AsNoTracking()
                .Where(log =>
                    log.Deleted != true &&
                    log.IssueDate >= dateStart &&
                    log.IssueDate <= dateEnd &&
                    (!filterCompany || (log.CompanyId ?? "") == companyParam)
                )
                .Join(_context.TrkRwPoheaders,
                      log => log.Ponum,
                      hdr => hdr.Ponum,
                      (log, hdr) => new { log, hdr })
                .Join(_context.TrkRwvendors,
                      combined => combined.hdr.VendorNum,
                      v => v.VendorNum,
                      (combined, v) => new
                      {
                          combined.log.Ponum,
                          combined.log.SalesOrderNum,
                          combined.log.ItemNum,
                          combined.log.SalesRep,
                          combined.log.IssuedBy,
                          combined.log.ExpectedDelivery,
                          combined.hdr.RequiredDate,
                          combined.log.QtyOrdered,
                          combined.log.QtyReceived,
                          combined.hdr.Postatus,
                          VendorName = v.VendorName ?? "",
                          HasNote = _context.TrkPonotes.Any(n => n.Ponum.ToString() == combined.log.Ponum)
                      });

            // Optional filters
            if (!string.IsNullOrEmpty(p.PONum))
                vendorsQuery = vendorsQuery.Where(x => x.Ponum.Contains(p.PONum));
            if (!string.IsNullOrEmpty(p.SONum))
                vendorsQuery = vendorsQuery.Where(x => x.SalesOrderNum.Contains(p.SONum));
            if (!string.IsNullOrEmpty(p.PartNum))
                vendorsQuery = vendorsQuery.Where(x => x.ItemNum.Contains(p.PartNum));
            if (!string.IsNullOrEmpty(p.xSalesRep)
                && !string.Equals(p.xSalesRep, "all", StringComparison.OrdinalIgnoreCase))
            {
                vendorsQuery = vendorsQuery.Where(x => x.SalesRep == p.xSalesRep);
            }
            if (!string.IsNullOrEmpty(p.IssuedBy)
                && !string.Equals(p.IssuedBy, "all", StringComparison.OrdinalIgnoreCase))
            {
                vendorsQuery = vendorsQuery.Where(x => x.IssuedBy == p.IssuedBy);
            }

            // HasNotes
            if (string.Equals(p.HasNotes, "yes", StringComparison.OrdinalIgnoreCase))
                vendorsQuery = vendorsQuery.Where(x => x.HasNote);
            else if (string.Equals(p.HasNotes, "no", StringComparison.OrdinalIgnoreCase))
                vendorsQuery = vendorsQuery.Where(x => !x.HasNote);

            // POStatus
            switch (p.POStatus.ToLowerInvariant())
            {
                case "not complete":
                    vendorsQuery = vendorsQuery.Where(x => x.QtyOrdered > x.QtyReceived && x.Postatus == 1);
                    break;
                case "complete":
                    vendorsQuery = vendorsQuery.Where(x => x.QtyOrdered <= x.QtyReceived);
                    break;
                case "late":
                    vendorsQuery = vendorsQuery.Where(x =>
                        x.QtyOrdered > x.QtyReceived &&
                        x.Postatus == 1 &&
                        (
                            (x.ExpectedDelivery.HasValue && DateTime.Now > x.ExpectedDelivery.Value && x.RequiredDate.HasValue && DateTime.Now > x.RequiredDate.Value) ||
                            (x.RequiredDate.HasValue && DateTime.Now > x.RequiredDate.Value && !x.ExpectedDelivery.HasValue)
                        )
                    );
                    break;
                case "due w/n 2 days":
                    vendorsQuery = vendorsQuery.Where(x =>
                        x.QtyOrdered > x.QtyReceived &&
                        x.Postatus == 1 &&
                        (
                            (x.ExpectedDelivery.HasValue && x.ExpectedDelivery.Value >= DateTime.Now && x.ExpectedDelivery.Value <= DateTime.Now.AddDays(2)) ||
                            (x.RequiredDate.HasValue && x.RequiredDate.Value >= DateTime.Now && x.RequiredDate.Value <= DateTime.Now.AddDays(2) && !x.ExpectedDelivery.HasValue)
                        )
                    );
                    break;
            }

            // EquipType
            if (string.Equals(p.EquipType, "anc", StringComparison.OrdinalIgnoreCase))
                vendorsQuery = vendorsQuery.Where(x => x.VendorName == "anc");
            else if (string.Equals(p.EquipType, "fne", StringComparison.OrdinalIgnoreCase))
                vendorsQuery = vendorsQuery.Where(x => x.VendorName == "fne");

            var vendors = await vendorsQuery
                .Select(x => x.VendorName)
                .Distinct()
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} Vendors after filtering", vendors.Count);
            return vendors;
        }
    }
}

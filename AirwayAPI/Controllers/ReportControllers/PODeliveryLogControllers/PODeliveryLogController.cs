using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers.PODeliveryLogControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODeliveryLogController(eHelpDeskContext context, ILogger<PODeliveryLogController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<PODeliveryLogController> _logger = logger;

        // GET: api/PODeliveryLog
        [HttpGet]
        public async Task<IActionResult> GetPODeliveryLogs(
            [FromQuery] string? PONum,
            [FromQuery] string? Vendor,
            [FromQuery] string? PartNum,
            [FromQuery] string? IssuedBy,
            [FromQuery] string? SONum,
            [FromQuery] string? xSalesRep,
            [FromQuery] string? HasNotes = "All",
            [FromQuery] string? POStatus = "Not Complete",
            [FromQuery] string? EquipType = "All",
            [FromQuery] string? CompanyID = "AIR",
            [FromQuery] int YearRange = 0
        )
        {
            if (YearRange == 0) YearRange = DateTime.Now.Year;

            var date1 = new DateTime(YearRange, 1, 1);
            var date2 = new DateTime(YearRange, 12, 31);

            _logger.LogInformation("Retrieving PODeliveryLogs for Year: {Year}, CompanyID: {CompanyID}", YearRange, CompanyID);

            var query = _context.TrkPologs
                .Where(log => log.Deleted != true
                    && log.IssueDate >= date1 && log.IssueDate <= date2
                    && (CompanyID.ToLower() == "all" || log.CompanyId.ToLower() == CompanyID.ToLower()))
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
                        .Select(n => n.EntryDate.HasValue ? n.EntryDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null)
                        .FirstOrDefault(),
                    PORequiredDate = log.RequiredDate,
                    DateDelivered = log.DateDelivered,
                    EditDate = log.EditDate,
                    ExpectedDelivery = log.ExpectedDelivery,
                    Sonum = log.SalesOrderNum ?? "",
                    IssuedBy = log.IssuedBy ?? "",
                    VendorName = _context.TrkRwPoheaders
                        .Where(p => p.Ponum == log.Ponum)
                        .Select(p => p.VendorNum)
                        .Distinct()
                        .Join(_context.TrkRwvendors, vn => vn, v => v.VendorNum, (vn, v) => v.VendorName)
                        .FirstOrDefault() ?? "",
                    ItemClassId = _context.TrkRwImItems
                        .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                        .Select(i => i.ItemClassId)
                        .FirstOrDefault(),
                    AltPartNum = _context.TrkRwImItems
                        .Where(i => i.ItemNum == log.ItemNum && i.CompanyId == log.CompanyId)
                        .Select(i => i.AltPartNum)
                        .FirstOrDefault() ?? "",
                    Postatus = _context.TrkRwPoheaders
                        .Where(p => p.Ponum == log.Ponum)
                        .Select(p => p.Postatus)
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
                    IsDropShipment = (_context.QtSalesOrders
                        .Where(q => q.RwsalesOrderNum == log.SalesOrderNum)
                        .OrderByDescending(q => q.SaleId)
                        .Select(q => q.DropShipment)
                        .FirstOrDefault()) == true
                });

            // Apply filtering based on user input
            if (!string.IsNullOrEmpty(PONum))
            {
                _logger.LogInformation("Applying PONum filter: {PONum}", PONum);
                query = query.Where(l => l.Ponum.Contains(PONum));
            }
            if (!string.IsNullOrEmpty(SONum))
            {
                _logger.LogInformation("Applying SONum filter: {SONum}", SONum);
                query = query.Where(l => l.Sonum.Contains(SONum));
            }
            if (!string.IsNullOrEmpty(Vendor))
            {
                _logger.LogInformation("Applying Vendor filter: {Vendor}", Vendor);
                query = query.Where(l => l.VendorName.Contains(Vendor));
            }
            if (!string.IsNullOrEmpty(PartNum))
            {
                _logger.LogInformation("Applying PartNum filter: {PartNum}", PartNum);
                query = query.Where(l => l.ItemNum.Contains(PartNum));
            }
            if (!string.IsNullOrEmpty(xSalesRep) && xSalesRep.ToLower() != "all")
            {
                _logger.LogInformation("Applying SalesRep filter: {SalesRep}", xSalesRep);
                query = query.Where(l => l.SalesRep.ToLower() == xSalesRep.ToLower());
            }

            // PO Status Filtering
            var normalizedPOStatus = POStatus.ToLower();
            switch (normalizedPOStatus)
            {
                case "not complete":
                    _logger.LogInformation("Applying POStatus filter: Not Complete");
                    query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1);
                    break;
                case "complete":
                    _logger.LogInformation("Applying POStatus filter: Complete");
                    query = query.Where(l => l.QtyOrdered <= l.QtyReceived);
                    break;
                case "late":
                    _logger.LogInformation("Applying POStatus filter: Late");
                    query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                        (((DateTime.Now > l.ExpectedDelivery) == true && (DateTime.Now > l.PORequiredDate) == true) ||
                         ((DateTime.Now > l.PORequiredDate) == true && l.ExpectedDelivery == null)));
                    break;
                case "due w/n 2 days":
                    _logger.LogInformation("Applying POStatus filter: Due w/n 2 days");
                    query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                        (((l.ExpectedDelivery >= DateTime.Now) == true && (l.ExpectedDelivery <= DateTime.Now.AddDays(2)) == true) ||
                         ((l.PORequiredDate >= DateTime.Now) == true && (l.PORequiredDate <= DateTime.Now.AddDays(2)) == true && l.ExpectedDelivery == null)));
                    break;
            }

            if (!string.IsNullOrEmpty(IssuedBy) && IssuedBy.ToLower() != "all")
            {
                _logger.LogInformation("Applying IssuedBy filter: {IssuedBy}", IssuedBy);
                query = query.Where(l => l.IssuedBy.ToLower() == IssuedBy.ToLower());
            }

            // Updated HasNotes filtering logic
            if (!string.IsNullOrEmpty(HasNotes))
            {
                var normalizedHasNotes = HasNotes.ToLower();
                _logger.LogInformation("Applying HasNotes filter: {HasNotes}", HasNotes);
                if (normalizedHasNotes == "yes")
                {
                    query = query.Where(l => l.NotesExist == true);
                }
                else if (normalizedHasNotes == "no")
                {
                    query = query.Where(l => l.NotesExist == false);
                }
            }

            if (EquipType.ToLower() == "anc")
            {
                _logger.LogInformation("Applying EquipType filter: Anc");
                query = query.Where(l => l.ItemClassId == 23);
            }
            else if (EquipType.ToLower() == "fne")
            {
                _logger.LogInformation("Applying EquipType filter: Fne");
                query = query.Where(l => l.ItemClassId == 24);
            }

            var results = await query.ToListAsync();
            _logger.LogInformation("Retrieved {Count} PODeliveryLogs after filtering", results.Count);
            return Ok(results);
        }

        [HttpGet("vendors")]
        public async Task<IActionResult> GetVendors(
            [FromQuery] string? PONum,
            [FromQuery] string? PartNum,
            [FromQuery] string? IssuedBy,
            [FromQuery] string? SONum,
            [FromQuery] string? xSalesRep,
            [FromQuery] string? HasNotes = "All",
            [FromQuery] string? POStatus = "Not Complete",
            [FromQuery] string? EquipType = "All",
            [FromQuery] string? CompanyID = "AIR",
            [FromQuery] int YearRange = 0
        )
        {
            if (YearRange == 0) YearRange = DateTime.Now.Year;
            var date1 = new DateTime(YearRange, 1, 1);
            var date2 = new DateTime(YearRange, 12, 31);

            var query = from log in _context.TrkPologs
                        join p in _context.TrkRwPoheaders on log.Ponum equals p.Ponum
                        join i in _context.TrkRwImItems on new { log.ItemNum, log.CompanyId } equals new { i.ItemNum, i.CompanyId }
                        join v in _context.TrkRwvendors on p.VendorNum equals v.VendorNum
                        let hasNote = _context.TrkPonotes.Any(n => n.Ponum.ToString() == log.Ponum)
                        where log.Deleted == false &&
                              log.IssueDate >= date1 && log.IssueDate <= date2
                        select new { log, p, i, v, HasNote = hasNote };

            // Apply CompanyID filter conditionally
            if (CompanyID.ToLower() != "all")
                query = query.Where(l => l.log.CompanyId.ToLower() == CompanyID.ToLower());

            // Apply additional filters
            if (!string.IsNullOrEmpty(PONum)) query = query.Where(l => l.log.Ponum.Contains(PONum));
            if (!string.IsNullOrEmpty(SONum)) query = query.Where(l => l.log.SalesOrderNum.Contains(SONum));
            if (!string.IsNullOrEmpty(PartNum)) query = query.Where(l => l.log.ItemNum.Contains(PartNum));
            if (!string.IsNullOrEmpty(xSalesRep) && xSalesRep.ToLower() != "all") query = query.Where(l => l.log.SalesRep.ToLower() == xSalesRep.ToLower());
            if (!string.IsNullOrEmpty(IssuedBy) && IssuedBy.ToLower() != "all") query = query.Where(l => l.log.IssuedBy.ToLower() == IssuedBy.ToLower());

            // Apply HasNotes filtering
            var normalizedHasNotes = HasNotes.ToLower();
            if (normalizedHasNotes == "yes")
            {
                query = query.Where(l => l.HasNote == true);
            }
            else if (normalizedHasNotes == "no")
            {
                query = query.Where(l => l.HasNote == false);
            }

            // PO Status Filtering
            var normalizedPOStatus = POStatus.ToLower();
            switch (normalizedPOStatus)
            {
                case "not complete":
                    query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1);
                    break;
                case "complete":
                    query = query.Where(l => l.log.QtyOrdered <= l.log.QtyReceived);
                    break;
                case "late":
                    query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1 &&
                        (DateTime.Now > l.log.ExpectedDelivery && DateTime.Now > l.log.RequiredDate ||
                        DateTime.Now > l.log.RequiredDate && l.log.ExpectedDelivery == null));
                    break;
                case "due w/n 2 days":
                    query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1 &&
                        (l.log.ExpectedDelivery >= DateTime.Now && l.log.ExpectedDelivery <= DateTime.Now.AddDays(2) ||
                        l.log.RequiredDate >= DateTime.Now && l.log.ExpectedDelivery == null &&
                         l.log.RequiredDate >= DateTime.Now && l.log.RequiredDate <= DateTime.Now.AddDays(2)));
                    break;
            }

            // EquipType Filtering
            if (EquipType.ToLower() == "anc")
                query = query.Where(l => l.i.ItemClassId == 23);
            else if (EquipType.ToLower() == "fne")
                query = query.Where(l => l.i.ItemClassId == 24);

            var vendors = await query
                .Select(l => l.v.VendorName)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();

            return Ok(vendors);
        }
    }
}

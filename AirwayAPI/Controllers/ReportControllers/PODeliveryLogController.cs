using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODeliveryLogController(eHelpDeskContext context, ILogger<PODeliveryLogController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<PODeliveryLogController> _logger = logger;

        /// <summary>
        /// Retrieves a list of Purchase Order (PO) delivery logs based on various filter criteria.
        /// This method provides detailed information about PO deliveries, including their status,
        /// associated vendors, items, and related sales orders.
        /// </summary>
        /// <param name="PONum">Filters logs by Purchase Order number (optional).</param>
        /// <param name="Vendor">Filters logs by vendor name (optional).</param>
        /// <param name="PartNum">Filters logs by part number (optional).</param>
        /// <param name="IssuedBy">Filters logs by the issuer's name (optional).</param>
        /// <param name="SONum">Filters logs by Sales Order number (optional).</param>
        /// <param name="xSalesRep">Filters logs by sales representative (optional).</param>
        /// <param name="HasNotes">Filters logs based on the existence of notes ("yes", "no", or "all"). Default is "All".</param>
        /// <param name="POStatus">Filters logs based on PO status ("Not Complete", "Complete", "Late", "Due w/n 2 days"). Default is "Not Complete".</param>
        /// <param name="EquipType">Filters logs by equipment type ("anc", "fne", or "All"). Default is "All".</param>
        /// <param name="CompanyID">Filters logs by company ID. Default is "AIR".</param>
        /// <param name="YearRange">Filters logs within the specified year. Default is the current year.</param>
        /// <returns>
        /// An asynchronous action result containing a list of PO delivery logs that match the filter criteria.
        /// Returns HTTP 200 OK with the results if successful.
        /// </returns>
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

            var dateStart = new DateTime(YearRange, 1, 1);
            var dateEnd = new DateTime(YearRange, 12, 31);

            _logger.LogInformation("Retrieving PODeliveryLogs for Year: {Year}, CompanyID: {CompanyID}", YearRange, CompanyID);

            var query = _context.TrkPologs
                .AsNoTracking()
                .Where(log => log.Deleted != true
                    && log.IssueDate >= dateStart
                    && log.IssueDate <= dateEnd
                    && (CompanyID.Equals("all", StringComparison.OrdinalIgnoreCase) || log.CompanyId.ToLower() == CompanyID.ToLower()))
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
                    IsDropShipment = _context.QtSalesOrders
                        .Where(q => q.RwsalesOrderNum == log.SalesOrderNum)
                        .OrderByDescending(q => q.SaleId)
                        .Select(q => q.DropShipment)
                        .FirstOrDefault() == true
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
            if (!string.IsNullOrEmpty(xSalesRep) && !xSalesRep.Equals("all", StringComparison.OrdinalIgnoreCase))
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
                        (
                            l.ExpectedDelivery.HasValue && DateTime.Now > l.ExpectedDelivery.Value &&
                             l.PORequiredDate.HasValue && DateTime.Now > l.PORequiredDate.Value ||
                            l.PORequiredDate.HasValue && DateTime.Now > l.PORequiredDate.Value && !l.ExpectedDelivery.HasValue
                        ));
                    break;
                case "due w/n 2 days":
                    _logger.LogInformation("Applying POStatus filter: Due w/n 2 days");
                    query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                        (
                            l.ExpectedDelivery.HasValue && l.ExpectedDelivery.Value >= DateTime.Now && l.ExpectedDelivery.Value <= DateTime.Now.AddDays(2) ||
                            l.PORequiredDate.HasValue && l.PORequiredDate.Value >= DateTime.Now && l.PORequiredDate.Value <= DateTime.Now.AddDays(2) && !l.ExpectedDelivery.HasValue
                        ));
                    break;
            }

            if (!string.IsNullOrEmpty(IssuedBy) && !IssuedBy.Equals("all", StringComparison.OrdinalIgnoreCase))
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

            if (EquipType.Equals("anc", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Applying EquipType filter: Anc");
                query = query.Where(l => l.ItemClassId == 23);
            }
            else if (EquipType.Equals("fne", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Applying EquipType filter: Fne");
                query = query.Where(l => l.ItemClassId == 24);
            }

            var results = await query.ToListAsync();
            _logger.LogInformation("Retrieved {Count} PODeliveryLogs after filtering", results.Count);
            return Ok(results);
        }

        /// <summary>
        /// Retrieves a distinct list of vendor names associated with Purchase Order (PO) delivery logs based on various filter criteria.
        /// This method is useful for populating vendor selection lists or analyzing vendor-related data.
        /// </summary>
        /// <param name="PONum">Filters logs by Purchase Order number (optional).</param>
        /// <param name="PartNum">Filters logs by part number (optional).</param>
        /// <param name="IssuedBy">Filters logs by the issuer's name (optional).</param>
        /// <param name="SONum">Filters logs by Sales Order number (optional).</param>
        /// <param name="xSalesRep">Filters logs by sales representative (optional).</param>
        /// <param name="HasNotes">Filters logs based on the existence of notes ("yes", "no", or "all"). Default is "All".</param>
        /// <param name="POStatus">Filters logs based on PO status ("Not Complete", "Complete", "Late", "Due w/n 2 days"). Default is "Not Complete".</param>
        /// <param name="EquipType">Filters logs by equipment type ("anc", "fne", or "All"). Default is "All".</param>
        /// <param name="CompanyID">Filters logs by company ID. Default is "AIR".</param>
        /// <param name="YearRange">Filters logs within the specified year. Default is the current year.</param>
        /// <returns>
        /// An asynchronous action result containing a distinct list of vendor names that match the filter criteria.
        /// Returns HTTP 200 OK with the list of vendors if successful.
        /// </returns>
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
            var dateStart = new DateTime(YearRange, 1, 1);
            var dateEnd = new DateTime(YearRange, 12, 31);

            _logger.LogInformation("Retrieving Vendors for Year: {Year}, CompanyID: {CompanyID}", YearRange, CompanyID);

            // Normalize CompanyID for case-insensitive comparison
            bool filterCompany = !string.IsNullOrEmpty(CompanyID) && !CompanyID.Equals("all", StringComparison.OrdinalIgnoreCase);
            string normalizedCompanyID = filterCompany ? CompanyID.ToLower() : string.Empty;

            // Start building the query
            var query = _context.TrkPologs
                .AsNoTracking()
                .Where(log => log.Deleted != true
                    && log.IssueDate >= dateStart
                    && log.IssueDate <= dateEnd
                    && (!filterCompany || log.CompanyId.ToLower() == normalizedCompanyID))
                .Join(_context.TrkRwPoheaders,
                      log => log.Ponum,
                      p => p.Ponum,
                      (log, p) => new { log, p })
                .Join(_context.TrkRwvendors,
                      combined => combined.p.VendorNum,
                      v => v.VendorNum,
                      (combined, v) => new { combined.log, combined.p, v })
                .Where(l =>
                    (string.IsNullOrEmpty(PONum) || l.log.Ponum.Contains(PONum)) &&
                    (string.IsNullOrEmpty(SONum) || l.log.SalesOrderNum.Contains(SONum)) &&
                    (string.IsNullOrEmpty(PartNum) || l.log.ItemNum.Contains(PartNum)) &&
                    (string.IsNullOrEmpty(xSalesRep) || xSalesRep.Equals("all", StringComparison.OrdinalIgnoreCase) || l.log.SalesRep.ToLower() == xSalesRep.ToLower()) &&
                    (string.IsNullOrEmpty(IssuedBy) || IssuedBy.Equals("all", StringComparison.OrdinalIgnoreCase) || l.log.IssuedBy.ToLower() == IssuedBy.ToLower())
                )
                .Select(l => new
                {
                    l.v.VendorName,
                    l.log.ExpectedDelivery,
                    l.p.RequiredDate, // Accessed from 'p'
                    l.log.QtyOrdered,
                    l.log.QtyReceived,
                    l.p.Postatus, // Accessed from 'p'
                    HasNote = _context.TrkPonotes.Any(n => n.Ponum.ToString() == l.log.Ponum)
                });

            // Apply HasNotes filter
            if (!string.IsNullOrEmpty(HasNotes))
            {
                var normalizedHasNotes = HasNotes.ToLower();
                _logger.LogInformation("Applying HasNotes filter: {HasNotes}", HasNotes);
                if (normalizedHasNotes == "yes")
                {
                    query = query.Where(l => l.HasNote);
                }
                else if (normalizedHasNotes == "no")
                {
                    query = query.Where(l => !l.HasNote);
                }
            }

            // Apply POStatus filter
            if (!string.IsNullOrEmpty(POStatus))
            {
                var normalizedPOStatus = POStatus.ToLower();
                _logger.LogInformation("Applying POStatus filter: {POStatus}", POStatus);
                switch (normalizedPOStatus)
                {
                    case "not complete":
                        query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1);
                        break;
                    case "complete":
                        query = query.Where(l => l.QtyOrdered <= l.QtyReceived);
                        break;
                    case "late":
                        _logger.LogInformation("Applying POStatus filter: Late");
                        query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                            (
                                l.ExpectedDelivery.HasValue && DateTime.Now > l.ExpectedDelivery.Value &&
                                 l.RequiredDate.HasValue && DateTime.Now > l.RequiredDate.Value ||
                                l.RequiredDate.HasValue && DateTime.Now > l.RequiredDate.Value && !l.ExpectedDelivery.HasValue
                            ));
                        break;
                    case "due w/n 2 days":
                        _logger.LogInformation("Applying POStatus filter: Due w/n 2 days");
                        query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                            (
                                l.ExpectedDelivery.HasValue && l.ExpectedDelivery.Value >= DateTime.Now && l.ExpectedDelivery.Value <= DateTime.Now.AddDays(2) ||
                                l.RequiredDate.HasValue && l.RequiredDate.Value >= DateTime.Now && l.RequiredDate.Value <= DateTime.Now.AddDays(2) && !l.ExpectedDelivery.HasValue
                            ));
                        break;
                    default:
                        _logger.LogWarning("Unknown POStatus filter value: {POStatus}", POStatus);
                        break;
                }
            }

            // Apply EquipType filter
            if (!string.IsNullOrEmpty(EquipType))
            {
                var normalizedEquipType = EquipType.ToLower();
                _logger.LogInformation("Applying EquipType filter: {EquipType}", EquipType);
                switch (normalizedEquipType)
                {
                    case "anc":
                        query = query.Where(l => l.VendorName != null && l.VendorName.ToLower() == "anc");
                        break;
                    case "fne":
                        query = query.Where(l => l.VendorName != null && l.VendorName.ToLower() == "fne");
                        break;
                        // Add other cases if necessary
                }
            }

            // Select distinct VendorNames
            var vendors = await query
                .Select(v => v.VendorName)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} Vendors after filtering", vendors.Count);
            return Ok(vendors);
        }
    }
}

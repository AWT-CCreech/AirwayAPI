using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.ReportControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PODeliveryLogController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public PODeliveryLogController(eHelpDeskContext context)
        {
            _context = context;
        }

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

            var query = from log in _context.TrkPologs
                        join p in _context.TrkRwPoheaders on log.Ponum equals p.Ponum
                        join i in _context.TrkRwImItems on new { log.ItemNum, log.CompanyId } equals new { i.ItemNum, i.CompanyId }
                        join v in _context.TrkRwvendors on p.VendorNum equals v.VendorNum
                        join so in _context.TrkRwSoheaders on log.SalesOrderNum equals so.OrderNum into soGroup
                        from so in soGroup.DefaultIfEmpty()
                            // Left outer join with qtSalesOrder
                        join qtsoTmp in _context.QtSalesOrders on log.SalesOrderNum equals qtsoTmp.RwsalesOrderNum into qtsoGroup
                        from qtso in qtsoGroup.OrderByDescending(q => q.SaleId).Take(1).DefaultIfEmpty()
                        let note = _context.TrkPonotes
                            .Where(n => n.Ponum.ToString() == log.Ponum)
                            .OrderByDescending(n => n.EntryDate)
                            .FirstOrDefault()
                        where log.Deleted == false
                            && log.IssueDate >= date1 && log.IssueDate <= date2
                            && (CompanyID == "All" || log.CompanyId == CompanyID)
                        select new PODeliveryLogSearchResult
                        {
                            Id = log.Id,
                            Ponum = log.Ponum ?? "",
                            IssueDate = log.IssueDate,
                            ItemNum = log.ItemNum ?? "",
                            QtyOrdered = log.QtyOrdered,
                            QtyReceived = log.QtyReceived,
                            ReceiverNum = log.ReceiverNum,
                            NotesExist = note != null,
                            NoteEditDate = note != null && note.EntryDate.HasValue
                                ? note.EntryDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                : null,
                            PORequiredDate = log.RequiredDate,
                            DateDelivered = log.DateDelivered,
                            EditDate = log.EditDate,
                            ExpectedDelivery = log.ExpectedDelivery,
                            Sonum = log.SalesOrderNum ?? "",
                            IssuedBy = log.IssuedBy ?? "",
                            VendorName = v.VendorName ?? "",
                            ItemClassId = i.ItemClassId,
                            AltPartNum = i.AltPartNum ?? "",
                            Postatus = p.Postatus,
                            CompanyId = log.CompanyId ?? "",
                            ContactId = log.ContactId,
                            SalesRep = log.SalesRep ?? "",
                            CustomerName = so.CustomerName ?? "",
                            SORequiredDate = so.RequiredDate,
                            IsDropShipment = qtso != null && qtso.DropShipment == true
                        };

            // Apply filtering based on user input
            if (!string.IsNullOrEmpty(PONum)) query = query.Where(l => l.Ponum.Contains(PONum));
            if (!string.IsNullOrEmpty(SONum)) query = query.Where(l => l.Sonum.Contains(SONum));
            if (!string.IsNullOrEmpty(Vendor)) query = query.Where(l => l.VendorName.Contains(Vendor));
            if (!string.IsNullOrEmpty(PartNum)) query = query.Where(l => l.ItemNum.Contains(PartNum));
            if (!string.IsNullOrEmpty(xSalesRep) && xSalesRep != "All") query = query.Where(l => l.SalesRep == xSalesRep);

            // PO Status Filtering
            if (POStatus == "Not Complete")
            {
                query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1);
            }
            else if (POStatus == "Complete")
            {
                query = query.Where(l => l.QtyOrdered <= l.QtyReceived);
            }
            else if (POStatus == "Late")
            {
                query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                    ((DateTime.Now > l.ExpectedDelivery && DateTime.Now > l.PORequiredDate) ||
                    (DateTime.Now > l.PORequiredDate && l.ExpectedDelivery == null)));
            }
            else if (POStatus == "Due w/n 2 Days")
            {
                query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 &&
                    ((l.ExpectedDelivery >= DateTime.Now && l.ExpectedDelivery <= DateTime.Now.AddDays(2)) ||
                    (l.PORequiredDate >= DateTime.Now && l.PORequiredDate <= DateTime.Now.AddDays(2) && l.ExpectedDelivery == null)));
            }

            if (!string.IsNullOrEmpty(IssuedBy) && IssuedBy != "All") query = query.Where(l => l.IssuedBy == IssuedBy);

            // Updated HasNotes filtering logic
            if (!string.IsNullOrEmpty(HasNotes))
            {
                if (HasNotes.ToLower() == "yes")
                {
                    query = query.Where(l => l.NotesExist == true); // Check for existence of notes
                }
                else if (HasNotes.ToLower() == "no")
                {
                    query = query.Where(l => l.NotesExist == false); // Check for absence of notes
                }
            }

            if (EquipType == "ANC") query = query.Where(l => l.ItemClassId == 23);
            else if (EquipType == "FNE") query = query.Where(l => l.ItemClassId == 24);

            var results = await query.ToListAsync();
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
                        where log.Deleted == false &&
                              log.IssueDate >= date1 && log.IssueDate <= date2 &&
                              (CompanyID == "All" || log.CompanyId == CompanyID)
                        select new
                        {
                            log,
                            p,
                            i,
                            v
                        };

            // Apply additional filters
            if (!string.IsNullOrEmpty(PONum))
                query = query.Where(l => l.log.Ponum.Contains(PONum));

            if (!string.IsNullOrEmpty(SONum))
                query = query.Where(l => l.log.SalesOrderNum.Contains(SONum));

            if (!string.IsNullOrEmpty(PartNum))
                query = query.Where(l => l.log.ItemNum.Contains(PartNum));

            if (!string.IsNullOrEmpty(xSalesRep) && xSalesRep != "All")
                query = query.Where(l => l.log.SalesRep == xSalesRep);

            if (!string.IsNullOrEmpty(IssuedBy) && IssuedBy != "All")
                query = query.Where(l => l.log.IssuedBy == IssuedBy);

            // PO Status Filtering
            if (POStatus == "Not Complete")
            {
                query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1);
            }
            else if (POStatus == "Complete")
            {
                query = query.Where(l => l.log.QtyOrdered <= l.log.QtyReceived);
            }
            else if (POStatus == "Late")
            {
                query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1 &&
                    ((DateTime.Now > l.log.ExpectedDelivery && DateTime.Now > l.log.RequiredDate) ||
                    (DateTime.Now > l.log.RequiredDate && l.log.ExpectedDelivery == null)));
            }
            else if (POStatus == "Due w/n 2 Days")
            {
                query = query.Where(l => l.log.QtyOrdered > l.log.QtyReceived && l.p.Postatus == 1 &&
                    ((l.log.ExpectedDelivery >= DateTime.Now && l.log.ExpectedDelivery <= DateTime.Now.AddDays(2)) ||
                    (l.log.RequiredDate >= DateTime.Now && l.log.RequiredDate <= DateTime.Now.AddDays(2) && l.log.ExpectedDelivery == null)));
            }

            // EquipType Filtering
            if (EquipType == "ANC")
                query = query.Where(l => l.i.ItemClassId == 23);
            else if (EquipType == "FNE")
                query = query.Where(l => l.i.ItemClassId == 24);

            // Now select vendor names
            var vendors = await query
                .Select(l => l.v.VendorName)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();

            return Ok(vendors);
        }
    }
}
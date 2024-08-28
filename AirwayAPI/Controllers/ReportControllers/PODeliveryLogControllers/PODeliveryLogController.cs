using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
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
            [FromQuery] string? HasNotes,
            [FromQuery] string? POStatus = "Not Complete",
            [FromQuery] string? EquipType = "All",
            [FromQuery] string? CompanyID = "AIR",
            [FromQuery] int lstYear = 0)
        {
            if (lstYear == 0) lstYear = DateTime.Now.Year;

            var date1 = new DateTime(lstYear, 1, 1);
            var date2 = new DateTime(lstYear, 12, 31);

            var query = from l in _context.TrkPologs
                        join p in _context.TrkRwPoheaders on l.Ponum equals p.Ponum
                        join i in _context.TrkRwImItems on new { l.ItemNum, p.CompanyId } equals new { i.ItemNum, i.CompanyId }
                        join v in _context.TrkRwvendors on p.VendorNum equals v.VendorNum
                        where l.Deleted == false &&
                              l.IssueDate >= date1 && l.IssueDate <= date2 &&
                              l.CompanyId == CompanyID
                        select new
                        {
                            l.Id,
                            l.Ponum,
                            l.IssueDate,
                            l.ItemNum,
                            l.QtyOrdered,
                            l.QtyReceived,
                            l.ReceiverNum,
                            l.Notes,
                            NoteEditDate = l.NoteEditDate ?? "",
                            l.RequiredDate,
                            l.DateDelivered,
                            l.EditDate,
                            l.ExpectedDelivery,
                            l.SalesOrderNum,
                            l.SalesRep,
                            l.IssuedBy,
                            v.VendorName,
                            i.ItemClassId,
                            i.AltPartNum,
                            p.Postatus
                        };

            // Apply Filters
            if (!string.IsNullOrEmpty(PONum))
            {
                query = query.Where(l => l.Ponum == PONum);
            }
            if (!string.IsNullOrEmpty(SONum))
            {
                query = query.Where(l => l.SalesOrderNum.Contains(SONum));
            }
            if (!string.IsNullOrEmpty(Vendor))
            {
                query = query.Where(l => l.VendorName == Vendor);
            }
            if (!string.IsNullOrEmpty(PartNum))
            {
                query = query.Where(l => l.ItemNum.Contains(PartNum));
            }
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
                query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 && (
                            (DateTime.Now > l.ExpectedDelivery && DateTime.Now > l.RequiredDate) ||
                            (DateTime.Now > l.RequiredDate && l.ExpectedDelivery == null)
                        ));
            }
            else if (POStatus == "Due w/n 2 Days")
            {
                query = query.Where(l => l.QtyOrdered > l.QtyReceived && l.Postatus == 1 && (
                            (l.ExpectedDelivery >= DateTime.Now && l.ExpectedDelivery <= DateTime.Now.AddDays(2)) ||
                            (l.RequiredDate >= DateTime.Now && l.RequiredDate <= DateTime.Now.AddDays(2) && l.ExpectedDelivery == null)
                        ));
            }
            if (!string.IsNullOrEmpty(IssuedBy) && IssuedBy != "All")
            {
                query = query.Where(l => l.IssuedBy == IssuedBy);
            }
            if (!string.IsNullOrEmpty(xSalesRep) && xSalesRep != "All")
            {
                query = query.Where(l => l.SalesRep == xSalesRep);
            }
            if (!string.IsNullOrEmpty(HasNotes))
            {
                if (HasNotes.ToLower() == "yes")
                {
                    query = query.Where(l => l.Notes != null && l.Notes.Trim().Length > 0);
                }
                else if (HasNotes.ToLower() == "no")
                {
                    query = query.Where(l => l.Notes == null || l.Notes.Trim().Length == 0);
                }
            }
            if (EquipType == "Ancillary")
            {
                query = query.Where(l => l.ItemClassId == 23);
            }
            else if (EquipType == "FNE")
            {
                query = query.Where(l => l.ItemClassId == 24);
            }

            var results = await query.OrderBy(l => l.Id).ToListAsync();
            return Ok(results);
        }
    }
}

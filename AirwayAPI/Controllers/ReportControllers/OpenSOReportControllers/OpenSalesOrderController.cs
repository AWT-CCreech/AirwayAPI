﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AirwayAPI.Data;
using AirwayAPI.Models;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenSalesOrderController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public OpenSalesOrderController(eHelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet("GetOpenSalesOrders")]
        public async Task<IActionResult> GetOpenSalesOrders(
            [FromQuery] string? soNum = "",
            [FromQuery] string? poNum = "",
            [FromQuery] string? custPO = "",
            [FromQuery] string? partNum = "",
            [FromQuery] string? reqDateStatus = "All",
            [FromQuery] string? salesTeam = "All",
            [FromQuery] string? category = "All",
            [FromQuery] string? salesRep = "All",
            [FromQuery] string? accountNo = "All",
            [FromQuery] string? customer = "",
            [FromQuery] bool chkExcludeCo = false,
            [FromQuery] bool chkGroupBySo = false,
            [FromQuery] bool chkAllHere = false,
            [FromQuery] string? dateFilterType = "OrderDate",
            [FromQuery] DateTime? date1 = null,
            [FromQuery] DateTime? date2 = null
        )
        {
            var query = _context.OpenSoreports.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(soNum))
            {
                query = query.Where(o => o.Sonum.Contains(soNum));
            }

            if (!string.IsNullOrEmpty(poNum))
            {
                query = query.Where(o => o.Ponum.Contains(poNum));
            }

            if (!string.IsNullOrEmpty(custPO))
            {
                query = query.Where(o => o.CustPo.Contains(custPO));
            }

            if (!string.IsNullOrEmpty(partNum))
            {
                query = query.Where(o => o.ItemNum.Contains(partNum));
            }

            if (reqDateStatus == "Late")
            {
                query = query.Where(o => o.RequiredDate < DateTime.Now);
            }

            if (salesTeam != "All")
            {
                query = query.Where(o => o.AccountTeam == salesTeam);
            }

            if (category != "All")
            {
                query = query.Where(o => o.Category == category);
            }

            if (salesRep != "All")
            {
                query = query.Where(o => o.SalesRep == salesRep);
            }

            if (accountNo != "All")
            {
                query = query.Where(o => o.AccountNo == accountNo);
            }

            if (!string.IsNullOrEmpty(customer))
            {
                if (chkExcludeCo)
                {
                    query = query.Where(o => !o.CustomerName.Contains(customer));
                }
                else
                {
                    query = query.Where(o => o.CustomerName.Contains(customer));
                }
            }

            if (dateFilterType == "OrderDate" && date1.HasValue && date2.HasValue)
            {
                query = query.Where(o => o.OrderDate >= date1.Value && o.OrderDate <= date2.Value);
            }
            else if (dateFilterType == "ExpectedDelivery" && date1.HasValue && date2.HasValue)
            {
                query = query.Where(o => o.RequiredDate >= date1.Value && o.RequiredDate <= date2.Value);
            }

            if (chkAllHere)
            {
                query = query.Where(o => o.AllHere == true);
            }

            var salesOrders = await query
                .Select(o => new
                {
                    o.EventId,
                    o.Sonum,
                    o.AccountTeam,
                    o.CustomerName,
                    o.CustPo,
                    o.OrderDate,
                    o.RequiredDate,
                    o.ItemNum,
                    o.MfgNum,
                    o.AmountLeft,
                    o.Ponum,
                    o.PoissueDate,
                    o.ExpectedDelivery,
                    o.QtyOrdered,
                    o.QtyReceived,
                    o.LeftToShip,
                    // Retrieve the latest PO Log entry
                    PoLog = (
                        from poLog in _context.TrkPologs
                        join poNote in _context.TrkPonotes on poLog.Ponum equals poNote.Ponum.ToString()
                        where poLog.Ponum == o.Ponum
                        orderby poNote.EntryDate descending
                        select new
                        {
                            poLog.Id,
                            poNote.EnteredBy,
                            poNote.EntryDate
                        }
                    ).FirstOrDefault(),
                    notes = _context.TrkSonotes
                        .Where(n => n.OrderNo == o.Sonum && n.PartNo == o.ItemNum)
                        .Select(n => new
                        {
                            n.Notes,
                            n.EntryDate,
                            n.EnteredBy,
                            n.ContactId,
                            ContactName = _context.CamContacts
                                            .Where(c => c.Id == n.ContactId)
                                            .Select(c => c.Contact)
                                            .FirstOrDefault() 
                        })
                        .ToList()
                })
                .OrderBy(o => o.Sonum)
                .ToListAsync();


            if (chkGroupBySo)
            {
                var groupedOrders = salesOrders
                    .GroupBy(o => o.Sonum)
                    .Select(g => g.First()) // Take the first order from each group
                    .ToList();

                return Ok(groupedOrders);
            }

            return Ok(salesOrders);
        }
    }
}

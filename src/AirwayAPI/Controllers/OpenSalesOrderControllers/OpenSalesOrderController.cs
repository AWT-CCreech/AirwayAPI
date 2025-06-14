﻿using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.OpenSalesOrderControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OpenSalesOrderController(eHelpDeskContext context) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;

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
        [FromQuery] string? dateFilterType = "SoTranDate",
        [FromQuery] DateTime? date1 = null,
        [FromQuery] DateTime? date2 = null
    )
    {
        // Cache the current time to avoid repeated evaluations.
        var now = DateTime.Now;

        // Use AsNoTracking for a read-only query.
        var query = _context.OpenSoreports
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(soNum))
        {
            query = query.Where(o => o.Sonum != null && EF.Functions.Like(o.Sonum, $"%{soNum}%"));
        }

        if (!string.IsNullOrEmpty(poNum))
        {
            query = query.Where(o => o.Ponum != null && EF.Functions.Like(o.Ponum, $"%{poNum}%"));
        }

        if (!string.IsNullOrEmpty(custPO))
        {
            query = query.Where(o => o.CustPo != null && EF.Functions.Like(o.CustPo, $"%{custPO}%"));
        }

        if (!string.IsNullOrEmpty(partNum))
        {
            query = query.Where(o => o.ItemNum != null && EF.Functions.Like(o.ItemNum, $"%{partNum}%"));
        }

        if (reqDateStatus == "Late")
        {
            query = query.Where(o => o.RequiredDate < now);
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
            query = chkExcludeCo
                ? query.Where(o => o.CustomerName != null && !EF.Functions.Like(o.CustomerName, $"%{customer}%"))
                : query.Where(o => o.CustomerName != null && EF.Functions.Like(o.CustomerName, $"%{customer}%"));
        }

        if (date1.HasValue && date2.HasValue)
        {
            if (dateFilterType == "SoTranDate")
            {
                query = query.Where(o => o.OrderDate >= date1.Value && o.OrderDate <= date2.Value);
            }
            else if (dateFilterType == "ExpectedDelivery")
            {
                query = query.Where(o => o.RequiredDate >= date1.Value && o.RequiredDate <= date2.Value);
            }
        }

        if (chkAllHere)
        {
            query = query.Where(o => (bool)o.AllHere);
        }

        // Project the query to include related data via subqueries.
        var salesOrdersQuery = query.Select(o => new
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
            PoLog = _context.TrkPologs
                .Where(poLog => poLog.Ponum == o.Ponum)
                .Join(_context.TrkPonotes,
                      poLog => poLog.Ponum,
                      poNote => poNote.Ponum.ToString(),
                      (poLog, poNote) => new
                      {
                          poLog.Id,
                          poNote.EnteredBy,
                          poNote.EntryDate
                      })
                .OrderByDescending(poNote => poNote.EntryDate)
                .FirstOrDefault(),
            notes = _context.TrkSonotes
                .Where(n => n.OrderNo == o.Sonum && n.PartNo == o.ItemNum)
                .Join(_context.CamContacts,
                      n => n.ContactId,
                      c => c.Id,
                      (n, c) => new
                      {
                          n.Notes,
                          n.EntryDate,
                          n.EnteredBy,
                          n.ContactId,
                          ContactName = c.Contact
                      })
                .ToList()
        })
        .OrderBy(o => o.Sonum);

        // Execute the query
        var salesOrders = await salesOrdersQuery.ToListAsync();

        // Optionally group by Sales Order number
        if (chkGroupBySo)
        {
            var groupedOrders = salesOrders
                .GroupBy(o => o.Sonum)
                .Select(g => g.First())
                .ToList();

            return Ok(groupedOrders);
        }

        return Ok(salesOrders);
    }
}

﻿using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.DropShipControllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DropShipInfoController(eHelpDeskContext context, ILogger<DropShipInfoController> logger) : ControllerBase
{
    private readonly eHelpDeskContext _context = context;
    private readonly ILogger<DropShipInfoController> _logger = logger;

    [HttpGet("{poNum}")]
    public async Task<ActionResult<object>> GetDropShipInfo(string poNum)
    {
        var SOs = await _context.QtSalesOrders
            .Where(so => so.RwsalesOrderNum != null && so.RwsalesOrderNum.Contains(poNum))
            .ToArrayAsync();

        int salesRepId = 0;
        for (var i = 0; i < SOs.Length; ++i)
        {
            var salesOrderNum = SOs[i].RwsalesOrderNum;
            if (salesOrderNum == poNum ||
                (salesOrderNum?.Contains(',') == true && salesOrderNum.Split(',').Contains(poNum)))
            {
                salesRepId = SOs[i].AccountMgr ?? 0;
            }
        }

        if (salesRepId != 0)
        {
            var salesRep = await _context.Users
                .Where(u => u.Id == salesRepId)
                .Select(u => new { u.Email, FullName = u.Fname + " " + u.Lname })
                .FirstOrDefaultAsync();
            return Ok(salesRep);
        }
        else
        {
            return Ok(null);
        }
    }

    [HttpGet("GetDropShipParts")]
    public async Task<IActionResult> GetDropShipParts([FromQuery] string? poNo = null, [FromQuery] string? soNo = null)
    {
        DateTime firstDayOfYear = new(DateTime.Now.Year, 1, 1);
        try
        {
            var query = from er in _context.EquipmentRequests.AsNoTracking()
                        where er.DropShipment == true && !string.IsNullOrEmpty(er.PartNum)
                        join sh in _context.ScanHistories.AsNoTracking()
                        on er.PartNum equals sh.PartNo
                        where !string.IsNullOrEmpty(sh.SerialNo)
                        && er.EntryDate > firstDayOfYear
                        let SoNo = string.IsNullOrEmpty(sh.SoNo) ? er.SalesOrderNum : sh.SoNo
                        join so in _context.TrkRwSoheaders on SoNo equals so.OrderNum into soGroup
                        from so in soGroup.DefaultIfEmpty()
                        orderby er.PartNum, sh.SerialNo
                        select new
                        {
                            PoNo = string.IsNullOrEmpty(sh.PoNo) ? "" : sh.PoNo,
                            SoNo,
                            PartNumber = er.PartNum,
                            SerialNumber = sh.SerialNo,
                            CustomerName = so != null ? so.CustomerName : null,
                            RequiredDate = so != null ? so.RequiredDate : null
                        };

            // Apply filters if provided
            if (!string.IsNullOrEmpty(poNo))
            {
                query = query.Where(x => x.PoNo == poNo);
            }

            if (!string.IsNullOrEmpty(soNo))
            {
                query = query.Where(x => x.SoNo == soNo);
            }

            var dropShipParts = await query.ToListAsync();

            return Ok(dropShipParts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drop ship parts");
            return StatusCode(500, "Internal server error");
        }
    }
}

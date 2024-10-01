using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class OpenSoreport
{
    public int Id { get; set; }

    public string? Sonum { get; set; }

    public string? CompanyId { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? CustomerName { get; set; }

    public string? AccountNo { get; set; }

    public string? CustPo { get; set; }

    public string? AccountTeam { get; set; }

    public string? SalesRep { get; set; }

    public int? QtyOrdered { get; set; }

    public int? QtyReceived { get; set; }

    public int? LeftToShip { get; set; }

    public int? OrgLeftToShip { get; set; }

    public string? ItemNum { get; set; }

    public string? MfgNum { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? AmountLeft { get; set; }

    public int? EventId { get; set; }

    public string? Category { get; set; }

    public string? Ponum { get; set; }

    public DateTime? PoissueDate { get; set; }

    public bool? Ponote { get; set; }

    public DateTime? PonoteDate { get; set; }

    public DateTime? ExpectedDelivery { get; set; }

    public bool? AllHere { get; set; }

    public bool? ReceivedOnPothatDay { get; set; }

    public DateTime? EntryDate { get; set; }
}

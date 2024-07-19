using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class CsQtSoToInvNo
{
    public int RowId { get; set; }

    public int? InvoiceNo { get; set; }

    public int? SalesOrderNo { get; set; }

    public int? OrgQuoteId { get; set; }

    public int? OrgEventId { get; set; }

    public byte? CompetitorFlag { get; set; }

    public string? CustAcctNo { get; set; }

    public string? SalesRep { get; set; }

    public string? SalesTeam { get; set; }

    public string? AccountMgr { get; set; }

    public string? AccountExec { get; set; }
}

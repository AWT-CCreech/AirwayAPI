using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class ScanTestLab
{
    public int RowId { get; set; }

    public int? ScanHistId { get; set; }

    public DateTime? ScanDate { get; set; }

    public string? UserName { get; set; }

    public string? ScannerId { get; set; }

    public string? PartNo { get; set; }

    public string? PartNo2 { get; set; }

    public string? PartNoClean { get; set; }

    public string? SerialNo { get; set; }

    public string? SerialNoB { get; set; }

    public string? HeciCode { get; set; }

    public int? Tag { get; set; }

    public string? Status { get; set; }

    public string? OrderType { get; set; }

    public int? OrderNo { get; set; }

    public string? TestResult { get; set; }

    public string? Notes { get; set; }

    public bool? EmailShipping { get; set; }

    public bool? EmailReceiving { get; set; }

    public bool? EmailPurchasing { get; set; }

    public bool? EmailSales { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public string? RedTagAction { get; set; }

    public string? RedTagStatus { get; set; }

    public string? EditBy { get; set; }

    public DateTime? EditDate { get; set; }

    public bool? Fn { get; set; }
}

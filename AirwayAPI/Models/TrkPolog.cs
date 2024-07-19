using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TrkPolog
{
    public int Id { get; set; }

    public string? Ponum { get; set; }

    public int? PolineKey { get; set; }

    public string? CompanyId { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpectedDelivery { get; set; }

    public string? ExpDelEditDate { get; set; }

    public string? SalesOrderNum { get; set; }

    public string? SalesRep { get; set; }

    public string? ItemNum { get; set; }

    public int? QtyOrdered { get; set; }

    public int? QtyReceived { get; set; }

    public int? ReceiverNum { get; set; }

    public DateTime? RequiredDate { get; set; }

    public string? IssuedBy { get; set; }

    public DateTime? DateDelivered { get; set; }

    public string? Notes { get; set; }

    public string? NoteEditDate { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? EditDate { get; set; }

    public int? EditedBy { get; set; }

    public bool? DeliveryDateEmail { get; set; }

    public int? ContactId { get; set; }
}

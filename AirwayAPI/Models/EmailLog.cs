﻿namespace AirwayAPI.Models;
// Utilized for the Outlook Email Logger add-in
public partial class EmailLog
{
    public int Id { get; set; }

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string OrderType { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public string SenderEmail { get; set; } = null!;

    public string? Recipients { get; set; }

    public string LoggedBy { get; set; } = null!;

    public DateTime? LoggedAt { get; set; }
}

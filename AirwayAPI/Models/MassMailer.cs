using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class MassMailer
{
    public int MassMailId { get; set; }

    public string? MassMailDesc { get; set; }

    public DateTime? DateSent { get; set; }

    public int? SentBy { get; set; }
}

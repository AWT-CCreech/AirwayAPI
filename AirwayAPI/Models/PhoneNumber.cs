using System;
using System.Collections.Generic;

namespace AirwayAPI.Data;

public partial class PhoneNumber
{
    public int Id { get; set; }

    public string? PhoneName { get; set; }

    public string? PhoneNumber1 { get; set; }

    public int UserId { get; set; }

    public string? Note { get; set; }
}

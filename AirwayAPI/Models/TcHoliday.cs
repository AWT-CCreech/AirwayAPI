using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class TcHoliday
{
    public int Id { get; set; }

    public string? Holiday { get; set; }

    public DateTime? HolidayDate { get; set; }

    public string? ImgFile { get; set; }
}

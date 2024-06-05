using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class Department
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    public string? DeptName { get; set; }

    public string? OpHours { get; set; }

    public int? LocationId { get; set; }

    public string? DeptEmail { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public int? MgrId { get; set; }
}

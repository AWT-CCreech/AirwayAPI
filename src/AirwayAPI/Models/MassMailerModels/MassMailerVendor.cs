﻿namespace AirwayAPI.Models.MassMailerModels;

public class MassMailerVendor
{
    public int Id { get; set; }
    public string Contact { get; set; }
    public string Email { get; set; }
    public string Company { get; set; }
    public bool? MainVendor { get; set; }
}

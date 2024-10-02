﻿namespace AirwayAPI.Models.PODeliveryLogModels
{
    public class PODetailUpdateDto
    {
        public int Id { get; set; }
        public string? Notes { get; set; } // Make Notes nullable
        public DateTime? ExpectedDelivery { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int ContactID { get; set; }
        public string PONum { get; set; }
        public string SONum {  get; set; }
        public string PartNum { get; set; }
        public bool UpdateAllDates { get; set; }
        public bool UrgentEmail { get; set; }
    }
}
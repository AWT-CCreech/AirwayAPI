namespace AirwayAPI.Models.MasterSearchModels
{
    public class SellOppDetail
    {
        public int EventId { get; set; }
        public int? ContactId { get; set; }
        public int? EnteredBy { get; set; }
        public int? RequestId { get; set; }
        public double? Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string PartNum { get; set; }
        public string AltPartNum { get; set; }
        public string PartDesc { get; set; }
        public bool? EquipFound { get; set; }
        public int QtFound { get; set; }
        public string Contact { get; set; }
        public string Company { get; set; }
        public string Uname { get; set; }
        public DateTime? EntryDate { get; set; }
        public int QuoteId { get; set; }
        public int? Version { get; set; }
    }
}

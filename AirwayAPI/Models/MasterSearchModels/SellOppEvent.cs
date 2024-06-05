namespace AirwayAPI.Models.MasterSearch
{
    public class SellOppEvent
    {
        public int EventId { get; set; }
        public int? ContactId { get; set; }
        public string SoldOrLost { get; set; }
        public int? EnteredBy { get; set; }
        public string Manufacturer { get; set; }
        public string Platform { get; set; }
        public string? EntryDate { get; set; }
        public string Contact { get; set; }
        public string Company { get; set; }
        public string Lname { get; set; }
        public int QuoteId { get; set; }
        public int? Version { get; set; }
    }
}

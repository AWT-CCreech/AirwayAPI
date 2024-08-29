namespace AirwayAPI.Models.MasterSearchModels
{
    public class BuyOppDetail
    {
        public int EventId { get; set; }
        public int DetailId { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public int? Quantity { get; set; }
        public string StatusCash { get; set; }
        public string StatusConsignment { get; set; }
        public DateTime? EntryDate { get; set; }
        public string Company { get; set; }
        public string Lname { get; set; }
        public string AskingPrice { get; set; }
    }
}

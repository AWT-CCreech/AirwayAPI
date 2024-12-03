using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Models.DropShipModels
{
    public class DropShipEmailInput : EmailInputBase
    {
        public List<string> RecipientNames { get; set; } = new List<string>();
        public string PONumber { get; set; }
        public string SONumber { get; set; }
        public string PartNumber { get; set; }
        public string Quantity { get; set; }
        public string Tracking { get; set; }
        public string SerialNumber { get; set; }
        public string Freight { get; set; }
    }
}

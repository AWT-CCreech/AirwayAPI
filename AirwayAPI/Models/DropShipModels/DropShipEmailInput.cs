namespace AirwayAPI.Data.DropShipModels
{
    public class DropShipEmailInput
    {
        public string Subject { get; set; }
        public string SenderUserName { get; set; }
        public string Password { get; set; }
        public string[] RecipientEmails { get; set; }
        public string[] RecipientNames { get; set; }
        public string PONumber { get; set; }
        public string SONumber { get; set; }
        public string PartNumber { get; set; }
        public string Quantity { get; set; }
        public string Tracking { get; set; }
        public string SerialNumber { get; set; }
        public string Freight { get; set; }
    }
}

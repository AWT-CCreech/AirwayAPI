namespace AirwayAPI.Models.PODeliveryLogModels
{
    public class PODetailEmailInput
    {
        public string FromEmail { get; set; } = "purch_dept@airway.com"; // Default email
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool Urgent { get; set; }
        public string SoNum { get; set; }
        public string SalesRep { get; set; }
        public string CompanyName { get; set; }
        public string SalesRequiredDate { get; set; }
        public string ExpectedDeliveryDate { get; set; }
        public string PartNumber { get; set; }
        public string Notes { get; set; }
    }
}

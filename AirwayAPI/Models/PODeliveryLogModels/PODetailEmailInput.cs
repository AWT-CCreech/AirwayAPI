using AirwayAPI.Models.EmailModels;

namespace AirwayAPI.Models.PODeliveryLogModels
{
    public class PODetailEmailInput : EmailInputBase
    {
        public PODetailEmailInput()
        {
            FromEmail = "purch_dept@airway.com"; // Default email
        }

        public string SoNum { get; set; }
        public string SalesRep { get; set; }
        public string CompanyName { get; set; }
        public string SalesRequiredDate { get; set; }
        public string ExpectedDeliveryDate { get; set; }
        public string PartNumber { get; set; }
        public string Notes { get; set; }
    }
}

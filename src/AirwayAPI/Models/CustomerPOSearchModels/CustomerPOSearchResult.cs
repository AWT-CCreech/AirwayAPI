namespace AirwayAPI.Models.CustomerPOSearchModels;

public class CustomerPOSearchResult
{
    public string SoTranNo { get; set; }
    public string CustPoNo { get; set; }
    public DateTime SoTranDate { get; set; }
    public decimal SoTranAmt { get; set; }
    public string CustomerName { get; set; }
    public string CustNum { get; set; }

    // Values populated from the secondary query
    public int EventID { get; set; }
    public int SaleID { get; set; }
    public int QuoteID { get; set; }
}

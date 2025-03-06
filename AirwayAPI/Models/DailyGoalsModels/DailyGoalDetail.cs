namespace AirwayAPI.Models.DailyGoalsModels
{
    public class DailyGoalDetail
    {
        public string OrderNum { get; set; }
        public string CustomerName { get; set; }
        public decimal QuoteTotal { get; set; }
        // For "Shipped" records, you may return additional cost information:
        public decimal? InvoiceCost { get; set; }
        public decimal? ConsignCost { get; set; }
    }
}

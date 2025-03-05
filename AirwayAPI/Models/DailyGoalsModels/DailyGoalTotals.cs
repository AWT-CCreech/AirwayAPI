namespace AirwayAPI.Models.DailyGoalsModels
{
    public class DailyGoalTotals
    {
        public decimal TotalSold { get; set; }
        public decimal TotalShipped { get; set; }
        public decimal TotalBackOrder { get; set; }
        public decimal SoBatchTotal { get; set; }
    }
}

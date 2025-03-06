namespace AirwayAPI.Models.DailyGoalsModels
{
    public class DailyGoalItem
    {
        public DateTime Date { get; set; }
        public decimal DailySold { get; set; }
        public decimal DailyShipped { get; set; }
        public decimal UnshippedBackOrder { get; set; }
    }
}

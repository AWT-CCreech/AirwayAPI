namespace AirwayAPI.Models.DailyGoalsModels
{
    public class DailyGoalsReport
    {
        public List<DailyGoalItem> Items { get; set; }
        public DailyGoalTotals Totals { get; set; }
    }
}

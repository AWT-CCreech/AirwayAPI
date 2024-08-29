namespace AirwayAPI.Models.TimeTrackerModels
{
    public class TimeTrackerReportEmailInput
    {
        public string Body { get; set; }
        public string SenderUserName { get; set; }
        public string Password { get; set; }
        public bool previousPeriod { get; set; }
    }
}

namespace AirwayAPI.Models.PortalModels
{
    public class PortalRouteDto
    {
        public int Id { get; set; }
        public string Path { get; set; } = null!;
        public string ComponentName { get; set; } = null!;
        public bool IsPrivate { get; set; }
        public int Ordering { get; set; }
    }
}

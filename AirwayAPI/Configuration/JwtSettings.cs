namespace AirwayAPI.Configuration
{
    public class JwtSettings
    {
        public required string Key { get; init; }
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public int TokenLifetimeMinutes { get; init; } = 30;
        public int RefreshLifetimeDays { get; init; } = 14;
    }
}

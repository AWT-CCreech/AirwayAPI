namespace AirwayAPI.Models.PortalModels
{
    public class PortalMenuItemDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
        public string? IconName { get; set; }
        public string? Path { get; set; }
        public string ItemType { get; set; } = null!;
        public int Ordering { get; set; }
        public int ColumnGroup { get; set; }
        public bool IsFavorite { get; set; }
        public List<PortalMenuItemDto> Children { get; set; } = [];
    }

}

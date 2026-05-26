namespace WarehouseApp.Models
{
    public class Award
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RequiredPoints { get; set; }
        public string? IconUrl { get; set; }
    }
}
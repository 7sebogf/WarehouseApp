namespace WarehouseApp.Models
{
    public class ReplenishmentPlan
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int PlannedQuantity { get; set; }
        public int? RecommendedQuantity { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PlannedDate { get; set; }
    }
}
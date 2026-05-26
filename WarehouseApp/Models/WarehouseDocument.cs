namespace WarehouseApp.Models
{
    public class WarehouseDocument
    {
        public int Id { get; set; }
        public int DocumentTypeId { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public int? WarehouseId { get; set; }
        public int? SupplierId { get; set; }
    }
}
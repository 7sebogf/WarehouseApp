namespace WarehouseApp.Models
{
    public class StockItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int? Quantity { get; set; }
        public int? ReservedQuantity { get; set; }
    }
}
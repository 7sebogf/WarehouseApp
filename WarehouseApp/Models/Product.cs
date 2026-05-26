#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Article { get; set; }

        public decimal Weight { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public decimal Price { get; set; }

        public int? StatusId { get; set; }
        public int? CategoryId { get; set; }
        public int? UnitId { get; set; }
        public int? SupplierId { get; set; }

        [MaxLength(100)]
        public string? CertificateCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        [ForeignKey("StatusId")]
        public virtual Status? Status { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }
    }
}
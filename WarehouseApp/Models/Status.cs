#nullable disable


using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Models
{
    public class Status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Color { get; set; }
    }
}
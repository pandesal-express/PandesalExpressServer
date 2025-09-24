using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("products")]
public class Product : Model
{
    [Column("category")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Required(ErrorMessage = "Category is required")]
    public required string Category { get; set; }

    [Column("name")]
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }

    [Column("price")]
    [Required(ErrorMessage = "Price is required")]
    public required decimal Price { get; set; }

    [Column("quantity")] public required int Quantity { get; set; }

    [Column("shift")]
    [Required(ErrorMessage = "Shift is required")]
    [StringLength(4, ErrorMessage = "Shift cannot exceed 4 characters")]
    public required string Shift { get; set; }

    [Column("description")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public ICollection<StoreInventory> StoreInventories { get; set; } = new HashSet<StoreInventory>();
    public ICollection<SalesLogItem> SalesLogItems { get; set; } = new HashSet<SalesLogItem>();
}

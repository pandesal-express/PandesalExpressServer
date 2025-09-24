using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("store_inventories")]
public class StoreInventory : Model
{
    [Column("store_id")] public Ulid StoreId { get; set; }

    [Column("product_id")] public Ulid ProductId { get; set; }

    [Column("quantity")] public required int Quantity { get; set; }

    [Column("price")] public required decimal Price { get; set; }

    [Column("last_verified")]
    [Description("The last time the inventory was verified between the delivery person and the store.")]
    public DateTime? LastVerified { get; set; }

    [ForeignKey("StoreId")] public Store? Store { get; set; }

    [ForeignKey("ProductId")] public Product? Product { get; set; }
}

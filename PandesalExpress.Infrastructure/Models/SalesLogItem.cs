using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("sales_log_items")]
public class SalesLogItem : Model
{
    [Column("sales_log_id")] public Ulid SalesLogId { get; set; }

    [Column("product_id")] public Ulid ProductId { get; set; }

    [Column("quantity")] public required int Quantity { get; set; }

    [Column("price_at_sale")]
    [Description("Price at the time of sale, which may differ from the current product price due to discounts or promotions.")]
    public decimal PriceAtSale { get; set; }

    [Column("amount")]
    [Description("Total amount for this item, calculated as Quantity * PriceAtSale.")]
    public required decimal Amount { get; set; }

    [ForeignKey("SalesLogId")] public SalesLog? SalesLog { get; set; }
    [ForeignKey("ProductId")] public Product? Product { get; set; }
}

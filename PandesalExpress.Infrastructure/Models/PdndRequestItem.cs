using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("pdnd_request_items")]
public class PdndRequestItem : Model
{
    [Column("pdnd_request_id")] public required Ulid PdndRequestId { get; set; }

    [Column("product_id")] public required Ulid ProductId { get; set; }

    [Column("product_name")] public required string ProductName { get; set; }

    [Column("quantity")] public required int Quantity { get; set; }

    [Column("total_amount")] public required decimal TotalAmount { get; set; }

    [ForeignKey("PdndRequestId")] public PdndRequest? PdndRequest { get; set; }
    [ForeignKey("ProductId")] public Product? Product { get; set; }
}

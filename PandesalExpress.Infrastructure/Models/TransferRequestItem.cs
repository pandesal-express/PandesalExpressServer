using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;


[Table("transfer_request_items")]
public sealed class TransferRequestItem : Model
{
	[Required]
	[Column("transfer_request_id")]
	public Ulid TransferRequestId { get; set; }

	[Required]
	[Column("product_id")]
	public Ulid ProductId { get; set; }

	[Column("product_name")]
	[StringLength(180)]
	public required string ProductName { get; set; }

	[Required]
	[Column("quantity_requested")]
	public int QuantityRequested { get; set; }

	[ForeignKey("TransferRequestId")]
	public TransferRequest? TransferRequest { get; set; }
	[ForeignKey("ProductId")]
	public Product? Product { get; set; }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

public enum TransferStatus
{
    Requested, // Store A created the request
    Accepted, // Store B acknowledged and agreed to send the items
    Rejected, // Store B cannot fulfill the request
    Shipped, // Store B's delivery personnel has picked up the items
    Received, // Store A's cashier has confirmed receipt of the items
    Cancelled // Initiating Store A cancelled the request before it was accepted
}

[Table("transfer_requests")]
public sealed class TransferRequest : Model
{
    [Required]
    [Column("sending_store_id")]
    public Ulid SendingStoreId { get; set; }

    [Required]
    [Column("receiving_store_id")]
    public Ulid ReceivingStoreId { get; set; }

    [Required]
    [Column("initiating_employee_id")]
    [Description("The cashier who made the request")]
    public Ulid InitiatingEmployeeId { get; set; }

    [Column("responding_employee_id")]
    [Description("The cashier who accepted/rejected the request")]
    public Ulid? RespondingEmployeeId { get; set; }

    [Required] [Column("status")] public TransferStatus Status { get; set; } = TransferStatus.Requested;

    [Column("request_notes")] public string? RequestNotes { get; set; } // Notes from the initiating cashier

    [Column("response_notes")] public string? ResponseNotes { get; set; } // Notes from the responding cashier (e.g., reason for rejection)

    [Column("system_message")] public string? SystemMessage { get; set; }

    [Column("shipped_at")] public DateTime? ShippedAt { get; set; } // Timestamp when items were picked up

    [Column("received_at")] public DateTime? ReceivedAt { get; set; } // Timestamp when items were received

    // Navigation Properties
    [ForeignKey("SendingStoreId")] public Store? SendingStore { get; set; }

    [ForeignKey("ReceivingStoreId")] public Store? ReceivingStore { get; set; }

    [ForeignKey("InitiatingEmployeeId")] public Employee? InitiatingEmployee { get; set; }

    [ForeignKey("RespondingEmployeeId")] public Employee? RespondingEmployee { get; set; }

    public ICollection<TransferRequestItem> Items { get; set; } = [];
}

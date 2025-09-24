using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos;

// The remaining product item in store
public class LeftOverProductDto
{
    [Required(ErrorMessage = "Product ID is required.")]
    public string ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

// The main request DTO for logging a sales transaction
public class LogSalesRequestDto
{
    public string StoreKey { get; set; }

    [Required(ErrorMessage = "At least one left over item is required.")]
    [MinLength(1, ErrorMessage = "At least one item must be in the transaction.")]
    public List<LeftOverProductDto> Items { get; set; } = [];

    [StringLength(50)] public string? Shift { get; set; } // e.g., "AM", "PM"
}

public class LogSalesResponseDto
{
    public required string SalesLogId { get; set; }
    public DateTime ServerTransactionTime { get; set; }
    public string Message { get; set; } = "Transaction logged successfully.";
    public int ItemsProcessed { get; set; }
    public decimal TotalAmount { get; set; }
}
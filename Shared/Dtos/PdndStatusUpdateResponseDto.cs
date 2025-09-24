namespace Shared.Dtos;

public record PdndStatusUpdateResponseDto
{
    public required string RequestId { get; init; }
    public required string PreviousStatus { get; init; }
    public required string NewStatus { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string UpdatedBy { get; init; }
    public string? Notes { get; init; }
    public string Message { get; init; } = "Status updated successfully";
}
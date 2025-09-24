using Shared.Dtos;

namespace PandesalExpress.PDND.Dtos;

public record PdndRequestsResponseDto
{
    public required List<PdndRequestDto> Requests { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
}
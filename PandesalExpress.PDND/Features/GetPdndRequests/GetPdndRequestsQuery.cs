using System.Security.Claims;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.PDND.Dtos;

namespace PandesalExpress.PDND.Features.GetPdndRequests;

public class GetPdndRequestsQuery : IQuery<PdndRequestsResponseDto>
{
    public required ClaimsPrincipal User { get; set; }
    public string? StoreId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

using System.Security.Claims;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Cashier.Features.LogSales;

public record LogSalesQuery(
    Ulid StoreUlid,
    ClaimsPrincipal User,
    LogSalesRequestDto RequestDto
) : IQuery<LogSalesResponseDto>;

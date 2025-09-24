using System.Security.Claims;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.PDND.Features.GetPdndRequest;

public record GetPdndRequestQuery(
    string RequestId,
    ClaimsPrincipal User
) : IQuery<PdndRequestDto>;
using System.Security.Claims;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Commissary.Features.AddStocksToStore;

public record AddStocksToStoreCommand(
    string StoreId,
    DeliverStocksRequestDto RequestDto,
    ClaimsPrincipal User
) : ICommand<AddStocksToStoreResponseDto>;

using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Transfers.Features.CreateTransferRequest;
using PandesalExpress.Transfers.Features.GetTransferRequest;
using PandesalExpress.Transfers.Features.GetTransferRequestsForStore;
using PandesalExpress.Transfers.Features.UpdateTransferRequestStatus;
using PandesalExpress.Transfers.Services;
using Shared.Dtos;

namespace PandesalExpress.Transfers;

public static class TransfersModuleServiceExtensions
{
    public static IServiceCollection AddTransfersModule(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ITransferStatusValidator, TransferStatusValidator>();
        services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();

        // command handlers
        services.AddScoped<
            ICommandHandler<CreateTransferRequestCommand, TransferRequestDto>,
            CreateTransferRequestHandler>();
        services.AddScoped<
            ICommandHandler<UpdateTransferRequestStatusCommand, TransferRequestDto>,
            UpdateTransferRequestStatusHandler>();

        // query handlers
        services.AddScoped<
            IQueryHandler<GetTransferRequestQuery, TransferRequestDto>,
            GetTransferRequestHandler>();
        services.AddScoped<
            IQueryHandler<GetTransferRequestsForStoreQuery, List<TransferRequestDto>>,
            GetTransferRequestsForStoreHandler>();

        return services;
    }
}

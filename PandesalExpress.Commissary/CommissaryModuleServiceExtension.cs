using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Commissary.Features.AddStocksToStore;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Commissary;

public static class CommissaryModuleServiceExtension
{
    public static IServiceCollection AddCommissaryModule(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<AddStocksToStoreCommand, AddStocksToStoreResponseDto>, AddStocksToStoreHandler>();

        return services;
    }
}

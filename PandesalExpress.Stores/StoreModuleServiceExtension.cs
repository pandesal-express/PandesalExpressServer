using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Stores.Features.GetStoreByKey;
using Shared.Dtos;

namespace PandesalExpress.Stores;

public static class StoresModuleServiceExtensions
{
    public static IServiceCollection AddStoresModule(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetStoreByKeyQuery, StoreDto?>, GetStoreByKeyQueryHandler>();

        return services;
    }
}

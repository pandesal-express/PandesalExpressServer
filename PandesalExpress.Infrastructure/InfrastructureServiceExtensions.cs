using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        return services;
    }
}
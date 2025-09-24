using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Cashier.Features.LogSales;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Cashier;

public static class CashierModuleServiceExtension
{
    public static IServiceCollection AddCashierModule(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<LogSalesQuery, LogSalesResponseDto>, LogSalesHandler>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.PDND.Dtos;
using PandesalExpress.PDND.Features.CreatePdndRequest;
using PandesalExpress.PDND.Features.GetPdndRequest;
using PandesalExpress.PDND.Features.GetPdndRequests;
using PandesalExpress.PDND.Features.UpdatePdndStatus;
using PandesalExpress.PDND.Services;
using Shared.Dtos;

namespace PandesalExpress.PDND;

public static class PdndModuleServiceExtension
{
    public static IServiceCollection AddPdndModule(this IServiceCollection services)
    {
        // Register command handlers
        services.AddScoped<ICommandHandler<CreatePdndRequestCommand, PdndRequestDto>, CreatePdndRequestHandler>();
        services.AddScoped<ICommandHandler<UpdatePdndStatusCommand, PdndStatusUpdateResponseDto>, UpdatePdndStatusHandler>();
        
        // Register query handlers
        services.AddScoped<IQueryHandler<GetPdndRequestsQuery, PdndRequestsResponseDto>, GetPdndRequestsHandler>();
        services.AddScoped<IQueryHandler<GetPdndRequestQuery, PdndRequestDto>, GetPdndRequestHandler>();
        
        // Register services
        services.AddScoped<IPdndStatusValidator, PdndStatusValidator>();

        return services;
    }
}

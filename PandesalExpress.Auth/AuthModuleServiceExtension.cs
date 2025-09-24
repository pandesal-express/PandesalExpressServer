using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Features.Login;
using PandesalExpress.Auth.Features.RefreshToken;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth;

public static class AuthModuleServiceExtension
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto>, LoginHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthResponseDto>, RefreshTokenHandler>();

        return services;
    }
}

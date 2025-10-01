using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Features.FaceLogin;
using PandesalExpress.Auth.Features.FaceRegister;
using PandesalExpress.Auth.Features.Login;
using PandesalExpress.Auth.Features.RefreshToken;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth;

public static class AuthModuleServiceExtension
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // TODO: Will remove the old standard auth in the future
        services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto>, LoginHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthResponseDto>, RefreshTokenHandler>();

        // Main authentication through face recognition
        services.AddScoped<ICommandHandler<FaceLoginCommand, AuthResponseDto>, FaceLoginHandler>();
        services.AddScoped<ICommandHandler<FaceRegisterCommand, AuthResponseDto>, FaceRegisterHandler>();

        return services;
    }
}

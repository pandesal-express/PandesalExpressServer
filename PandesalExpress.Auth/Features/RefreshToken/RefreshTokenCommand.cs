using Microsoft.AspNetCore.Http;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth.Features.RefreshToken;

public record RefreshTokenCommand(
    string ExpiredAccessToken,
    string RefreshToken,
    Action<string, string, CookieOptions> AppendCookie
) : ICommand<AuthResponseDto>;

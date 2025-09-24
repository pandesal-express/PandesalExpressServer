using Microsoft.AspNetCore.Http;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;

namespace PandesalExpress.Auth.Features.Login;

public record LoginCommand(
    string Email,
    string Password,
    Action<string, string, CookieOptions> AppendCookie
) : ICommand<AuthResponseDto>;

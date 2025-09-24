using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Auth.Features.Login;

public class LoginHandler(
    UserManager<Employee> userManager,
    ITokenService tokenService,
    ILogger<LoginHandler> logger
) : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Employee? employee = await userManager.Users
                                              .Include(e => e.Department)
                                              .FirstOrDefaultAsync(e => e.Email == request.Email, cancellationToken);

        if (employee == null || !await userManager.CheckPasswordAsync(employee, request.Password))
        {
            logger.LogWarning("Login failed for user: {Email}. Invalid credentials.", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (employee.Department == null)
        {
            logger.LogError("User {Email} is missing department information.", request.Email);
            throw new InvalidOperationException("User account is not configured correctly.");
        }

        // --- Generate Tokens and Claims ---
        (string accessToken, DateTime expiration) = await tokenService.GenerateJwtTokenAsync(employee);
        string refreshToken = tokenService.GenerateRefreshToken();

        // --- Update User's Refresh Token in DB ---
        employee.RefreshToken = refreshToken;
        employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(employee);

        // --- Set HttpOnly Cookies via the delegate passed from the controller ---
        request.AppendCookie(
            "jwt_token",
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiration,
                Path = "/"
            }
        );

        request.AppendCookie(
            "refresh_token",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = employee.RefreshTokenExpiryTime,
                Path = "/api/Auth/refresh-token"
            }
        );

        return new AuthResponseDto
        {
            Token = accessToken,
            Expiration = expiration,
            User = new EmployeeDto
            {
                Id = employee.Id.ToString(),
                Email = employee.Email!,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                Department = new DepartmentDto
                {
                    Id = employee.Department.Id.ToString(),
                    Name = employee.Department.Name
                }
            }
        };
    }
}

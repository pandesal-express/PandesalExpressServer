using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Exceptions;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Auth.Features.RefreshToken;

public class RefreshTokenHandler(
    UserManager<Employee> userManager,
    ITokenService tokenService,
    ILogger<RefreshTokenHandler> logger
) : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal principal = tokenService.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        // Get user from claims
        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Ulid.TryParse(userId, out Ulid userUlid))
        {
            logger.LogWarning("User ID is null or empty");
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        // Find user
        Employee? employee = await userManager.Users
                                              .Include(e => e.Department)
                                              .FirstOrDefaultAsync(u => u.Id == userUlid, cancellationToken);

        if (employee == null ||
            employee.RefreshToken != request.RefreshToken ||
            employee.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token is invalid, mismatch, or expired");
            throw new UnauthorizedAccessException("Invalid refresh token or token expired");
        }

        // Generate new tokens
        (string token, DateTime expiration) = await tokenService.GenerateJwtTokenAsync(employee);
        string newRefreshToken = tokenService.GenerateRefreshToken();

        // Update refresh token in database
        employee.RefreshToken = newRefreshToken;
        employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        IdentityResult result = await userManager.UpdateAsync(employee);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to update user's refresh token during refresh process for user {UserId}.", employee.Id);
            throw new UserManagerException("Could not update user session.");
        }

        DateTime refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

        request.AppendCookie(
            "jwt_token",
            token,
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
            newRefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = refreshTokenExpiration,
                Path = "/api/Auth/refresh-token"
            }
        );

        // Get employee data for response
        var employeeDto = new EmployeeDto
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
        };

        return new AuthResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = employeeDto
        };
    }
}

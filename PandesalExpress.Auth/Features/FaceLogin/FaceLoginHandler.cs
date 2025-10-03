using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Auth.Features.FaceLogin;

public class FaceLoginHandler(
    AppDbContext context,
    UserManager<Employee> userManager,
    ITokenService tokenService,
    ILogger<FaceLoginHandler> logger
) : ICommandHandler<FaceLoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(FaceLoginCommand command, CancellationToken cancellationToken)
    {
        Employee? employee = await userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (employee == null)
        {
            logger.LogWarning("Face login failed for user: User not found.");
            throw new UnauthorizedAccessException("User not found.");
        }

        logger.LogInformation("Generating tokens for user: {Email}", employee.Email);

        (string accessToken, DateTime expiration) = await tokenService.GenerateJwtTokenAsync(employee);
        string refreshToken = tokenService.GenerateRefreshToken();

        employee.RefreshToken = refreshToken;
        employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(3);

        var attendance = new Attendance
        {
            Id = Ulid.NewUlid(),
            EmployeeId = employee.Id,
            Status = AttendanceStatus.Present,
            CheckIn = command.TimeLogged.TimeOfDay
        };

        await userManager.UpdateAsync(employee);
        await context.Attendances.AddAsync(attendance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Face login successful for user: {Email}", employee.Email);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = expiration,
            RefreshTokenExpiration = employee.RefreshTokenExpiryTime?.ToString("O"),
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

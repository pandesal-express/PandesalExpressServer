using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Auth.Features.FaceRegister;

public class FaceRegisterHandler(
    UserManager<Employee> userManager,
    RoleManager<AppRole> roleManager,
    AppDbContext context,
    ITokenService tokenService,
    ILogger<FaceRegisterHandler> logger
) : ICommandHandler<FaceRegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(FaceRegisterCommand command, CancellationToken cancellationToken)
    {
        Employee? existingUser = await userManager.FindByEmailAsync(command.Dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        var departmentUlid = Ulid.Parse(command.Dto.DepartmentId);

        Department? department = await context.Departments
                                              .AsNoTracking()
                                              .Where(d => d.Id == departmentUlid)
                                              .Select(d => new Department
                                                  {
                                                      Id = d.Id,
                                                      Name = d.Name
                                                  }
                                              )
                                              .FirstOrDefaultAsync(cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("The specified department does not exist.");

        var employee = new Employee
        {
            Id = Ulid.NewUlid(),
            UserName = command.Dto.Email,
            Email = command.Dto.Email,
            FirstName = command.Dto.FirstName,
            LastName = command.Dto.LastName,
            Position = command.Dto.Position,
            DepartmentId = department.Id,
            Department = department
        };

        IdentityResult result = await userManager.CreateAsync(employee);
        if (!result.Succeeded)
        {
            var errorString = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new DataException($"User registration failed: {errorString}");
        }

        // --- Add user to a role based on department name ---
        if (!await roleManager.RoleExistsAsync(department.Name))
            await roleManager.CreateAsync(new AppRole(department.Name));

        await userManager.AddToRoleAsync(employee, department.Name);

        if (!await roleManager.RoleExistsAsync(employee.Position))
            await roleManager.CreateAsync(new AppRole(employee.Position));

        await userManager.AddToRoleAsync(employee, employee.Position);

        // Then insert new data to attendance
        var attendance = new Attendance
        {
            Id = Ulid.NewUlid(),
            EmployeeId = employee.Id,
            Status = AttendanceStatus.Present,
            CheckIn = command.Dto.TimeLogged.TimeOfDay // Get the TimeLogged from the command
        };

        await context.Attendances.AddAsync(attendance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User registered successfully: {Email}", employee.Email);

        (string accessToken, DateTime expiration) = await tokenService.GenerateJwtTokenAsync(employee);
        string refreshToken = tokenService.GenerateRefreshToken();

        employee.RefreshToken = refreshToken;
        employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(3);
        await userManager.UpdateAsync(employee);

        logger.LogInformation("Tokens generated for user: {Email}", employee.Email);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = expiration,
            RefreshTokenExpiration = employee.RefreshTokenExpiryTime?.ToString("O"),
            User = new EmployeeDto
            {
                Id = employee.Id.ToString(),
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                Department = new DepartmentDto
                {
                    Id = department.Id.ToString(),
                    Name = department.Name
                }
            }
        };
    }
}

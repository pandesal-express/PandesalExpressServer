using Microsoft.AspNetCore.Identity;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.Auth.Features.Register;

public class RegisterHandler(
    UserManager<Employee> userManager,
    RoleManager<AppRole> roleManager,
    AppDbContext context
) : ICommandHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        Employee? existingUser = await userManager.FindByEmailAsync(request.Dto.Email);
        if (existingUser != null) throw new InvalidOperationException("An account with this email already exists.");

        Department? department = await context.Departments.FindAsync([Ulid.Parse(request.Dto.DepartmentId)], cancellationToken);
        if (department == null) throw new KeyNotFoundException("The specified department does not exist.");

        var newEmployee = new Employee
        {
            Id = Ulid.NewUlid(),
            UserName = request.Dto.Email,
            Email = request.Dto.Email,
            FirstName = request.Dto.FirstName,
            LastName = request.Dto.LastName,
            Position = request.Dto.Position,
            DepartmentId = department.Id,
            Department = department,
            StoreId = string.IsNullOrEmpty(request.Dto.StoreId) ? null : Ulid.Parse(request.Dto.StoreId)
        };

        IdentityResult result = await userManager.CreateAsync(newEmployee, request.Dto.Password);
        if (!result.Succeeded)
        {
            var errorString = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"User creation failed: {errorString}");
        }

        // --- Add user to a role based on department name ---
        if (!await roleManager.RoleExistsAsync(department.Name))
            await roleManager.CreateAsync(new AppRole(department.Name));

        await userManager.AddToRoleAsync(newEmployee, department.Name);

        if (!await roleManager.RoleExistsAsync(newEmployee.Position))
            await roleManager.CreateAsync(new AppRole(newEmployee.Position));
        await userManager.AddToRoleAsync(newEmployee, newEmployee.Position);


        // --- After successful registration, automatically log the user in ---
        // To do this without duplicating logic, we can send a LoginCommand.
        // However, we need access to Response.Cookies.Append.
        // It's cleaner here to just call the TokenService directly since we have the user object.
        // Let's assume LoginCommandHandler is more appropriate to handle this.
        // For simplicity here, we'll return a success DTO without the tokens,
        // requiring the user to login manually after registration. Or, we can duplicate token logic.

        // Recommended: For a better UX, auto-login is great. But this requires passing the `AppendCookie`
        // action here too, which starts to couple things. A simpler approach for the API is to
        // just confirm registration and have the client redirect to login.

        return new AuthResponseDto // Return a limited response
        {
            Token = string.Empty,
            Expiration = DateTime.UtcNow,
            User = new EmployeeDto
            {
                Id = newEmployee.Id.ToString(),
                Email = newEmployee.Email,
                FirstName = newEmployee.FirstName,
                LastName = newEmployee.LastName,
                Position = newEmployee.Position,
                Department = new DepartmentDto
                {
                    Id = department.Id.ToString(),
                    Name = department.Name
                }
            }
        };
    }
}

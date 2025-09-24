using System.ComponentModel.DataAnnotations;
using Shared.Dtos;

// Assuming your EmployeeDto lives here

namespace PandesalExpress.Auth.Dtos;

public record LoginRequestDto
{
    [Required] [EmailAddress] public required string Email { get; init; }

    [Required] [MinLength(8)] public required string Password { get; init; }
}

public record RegisterRequestDto
{
    [Required] public required string FirstName { get; init; }
    [Required] public required string LastName { get; init; }
    [Required] [EmailAddress] public required string Email { get; init; }
    [Required] public required string Position { get; init; }
    [Required] public required string DepartmentId { get; init; }

    [Required] [MinLength(8)] public required string Password { get; init; }
    [Required] [Compare(nameof(Password))] public required string ConfirmPassword { get; init; }

    public string? StoreId { get; init; }
}

public record AuthResponseDto
{
    public required string Token { get; init; }
    public DateTime Expiration { get; init; }

    public required EmployeeDto User { get; init; }
}

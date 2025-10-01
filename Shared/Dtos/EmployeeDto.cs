namespace Shared.Dtos;

public class EmployeeDto
{
    public required string Id { get; set; }
    public string? DepartmentId { get; set; }
    public string? StoreId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }

    public DepartmentDto? Department { get; set; }
    public StoreDto? Store { get; set; }
}

namespace Shared.Dtos;

public class DepartmentDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    public List<EmployeeDto> Employees { get; set; } = [];
}

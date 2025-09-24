using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("departments")]
public class Department : Model
{
    [Column("name")]
    [StringLength(90, ErrorMessage = "Department name is too long, it should be less than 90 characters")]
    [Required(ErrorMessage = "Department name is required")]
    public required string Name { get; set; }

    public ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}

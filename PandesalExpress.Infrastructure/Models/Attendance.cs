using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

public enum AttendanceStatus
{
    Pending,
    Present,
    Absent,
    Late,
    Early
}

[Table("attendances")]
public class Attendance : Model
{
    [Column("employee_id")] public Ulid EmployeeId { get; set; }
    [Column("check_in")] public TimeSpan? CheckIn { get; set; }
    [Column("check_out")] public TimeSpan? CheckOut { get; set; }

    [Column("status")]
    [Required(ErrorMessage = "Status is required")]
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Pending;

    [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }
}

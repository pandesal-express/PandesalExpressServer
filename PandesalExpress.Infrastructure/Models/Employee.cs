using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PandesalExpress.Infrastructure.Models;

[Table("employees")]
[Index(nameof(FirstName), nameof(LastName))]
[Index(nameof(Email), IsUnique = true)]
public class Employee : IdentityUser<Ulid>
{
    [Column("department_id")] public Ulid DepartmentId { get; set; }

    [Column("store_id")] public Ulid? StoreId { get; set; }

    [Column("firstname")]
    [StringLength(70, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 70 characters")]
    [Required(ErrorMessage = "First name is required")]
    public required string FirstName { get; set; }

    [Column("lastname")]
    [StringLength(70, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 70 characters")]
    [Required(ErrorMessage = "Last name is required")]
    public required string LastName { get; set; }

    [Column("position")]
    [StringLength(180, ErrorMessage = "Position must be less than 180 characters")]
    [Required(ErrorMessage = "Position is required")]
    public required string Position { get; set; }

    [Column("refresh_token")] public string? RefreshToken { get; set; }
    [Column("refresh_token_expiry_time")] public DateTime? RefreshTokenExpiryTime { get; set; }

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("xmin", TypeName = "xid")]
    public uint RowVersion { get; set; }

    [ForeignKey("DepartmentId")] public required Department Department { get; set; }

    [ForeignKey("StoreId")] public Store? Store { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = [];
    public ICollection<SalesLog>? SalesLogProcessed { get; set; } = [];
    public ICollection<PdndRequest> PdndRequests { get; set; } = [];
}

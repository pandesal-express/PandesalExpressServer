using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("sales_logs")]
public class SalesLog : Model
{
    [Column("store_id")] public Ulid StoreId { get; set; }

    [Column("employee_id")] public Ulid EmployeeId { get; set; }

    [Column("name")] public required string Name { get; set; }

    [Column("quantity")] public required int Quantity { get; set; }

    [Column("total_price")] public required decimal TotalPrice { get; set; }

    [Column("shift")]
    [StringLength(15, ErrorMessage = "Shift cannot exceed 10 characters")]
    public string? Shift { get; set; }

    [ForeignKey("StoreId")] public Store? Store { get; set; }
    [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }

    public ICollection<SalesLogItem> SalesLogItems { get; set; } = new HashSet<SalesLogItem>();
}

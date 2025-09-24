using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("stores")]
public class Store : Model
{
    [Column("store_key")]
    [Required(ErrorMessage = "Store key is required")]
    public required string StoreKey { get; set; }

    [Column("name")]
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }

    [Column("address")]
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Description("The physical address of the store, including street, city, etc.")]
    public required string Address { get; set; }

    [Column("stocks_date_verified")]
    [Description("The date when the stock was last verified. This is used to ensure that the stock levels are accurate and up-to-date.")]
    public DateTime? StocksDateVerified { get; set; }

    [Column("opening_time", TypeName = "time")]
    public required TimeSpan OpeningTime { get; set; }

    [Column("closing_time", TypeName = "time")]
    public required TimeSpan ClosingTime { get; set; }

    public ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    public ICollection<StoreInventory> StoreInventories { get; set; } = new HashSet<StoreInventory>();
    public ICollection<SalesLog> SalesLogs { get; set; } = new HashSet<SalesLog>();
    public ICollection<Attendance> Attendances { get; set; } = new HashSet<Attendance>();
    public ICollection<PdndRequest> PdndRequests { get; set; } = new HashSet<PdndRequest>();
}

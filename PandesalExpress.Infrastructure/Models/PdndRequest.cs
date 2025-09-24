using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("pdnd_requests")]
public class PdndRequest : Model
{
	[Column("store_id")]
	public required Ulid StoreId { get; set; }

	[Column("requesting_employee_id")]
	public required Ulid RequestingEmployeeId { get; set; }

	[Column("commissary_id")]
	public Ulid? CommissaryId { get; set; }

	[Column("request_date")]
	public required DateTime RequestDate { get; set; }

	[Column("date_needed")]
	public required DateTime DateNeeded { get; set; }

	[Column("status")]
	[MaxLength(15)]
	public required string Status { get; set; }

	[Column("commissary_notes")]
	[MaxLength(500)]
	public string? CommissaryNotes { get; set; }

	[Column("status_last_updated")]
	public DateTime? StatusLastUpdated { get; set; }

	[Column("last_updated_by")]
	public Ulid? LastUpdatedBy { get; set; }

	[ForeignKey("StoreId")] public Store? Store { get; set; }
	[ForeignKey("RequestingEmployeeId")] public Employee? RequestingEmployee { get; set; }
	[ForeignKey("CommissaryId")] public Employee? Commissary { get; set; }
	[ForeignKey("LastUpdatedBy")] public Employee? LastUpdatedByEmployee { get; set; }

	public ICollection<PdndRequestItem> PdndRequestItems { get; set; } = new HashSet<PdndRequestItem>();
}
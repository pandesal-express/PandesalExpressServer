using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandesalExpress.Infrastructure.Models;

[Table("payrolls")]
public class Payroll : Model
{
    [Column("employee_id")] public Ulid EmployeeId { get; set; }

    [Column("base_salary")]
    [Required(ErrorMessage = "Base salary is required")]
    public required decimal BaseSalary { get; set; }

    [Column("tax")]
    [Required(ErrorMessage = "Tax is required")]
    public required decimal Tax { get; set; }

    [Column("sss_deduction")]
    [Required(ErrorMessage = "SSS deduction is required")]
    public required decimal SssDeduction { get; set; }

    [Column("philhealth_deduction")] public decimal PhilHealthDeduction { get; set; }
    [Column("pagibig_deduction")] public decimal PagIbigDeduction { get; set; }
    [Column("loan_deduction")] public decimal LoanDeduction { get; set; }

    [Column("overtime")]
    [Required(ErrorMessage = "Overtime is required")]
    [Description("Overtime in percentage")]
    public decimal Overtime { get; set; }

    [Column("bonus")]
    [Required(ErrorMessage = "Bonus is required")]
    public decimal Bonus { get; set; }

    [Column("total_salary")]
    [Required(ErrorMessage = "Total salary is required")]
    public decimal TotalSalary { get; set; }

    [ForeignKey("EmployeeId")] public Employee Employee { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.PDND.Dtos;

public class CreatePdndRequestDto
{
    [Required] public required string BranchCode { get; init; }
    [Required] public required string CashierId { get; init; }
    [Required] public required DateTime DateNeeded { get; init; }
    [Required] public required List<Product> Items { get; init; } = [];
}

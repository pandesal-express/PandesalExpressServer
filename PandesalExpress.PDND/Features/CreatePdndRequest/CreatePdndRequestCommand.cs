using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.PDND.Features.CreatePdndRequest;

public record CreatePdndRequestCommand(string StoreId, string StoreKey, string CashierId, DateTime DateNeeded, List<Product> Items) 
    : ICommand<PdndRequestDto>;

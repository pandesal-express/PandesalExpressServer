using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Transfers.Dtos;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Features.CreateTransferRequest;

public class CreateTransferRequestCommand(CreateTransferRequestDto createTransferRequestDto, Ulid initiatingEmployeeId) 
    : ICommand<TransferRequestDto>
{
    public CreateTransferRequestDto CreateTransferRequestDto { get; set; } = createTransferRequestDto;
    public Ulid InitiatingEmployeeId { get; set; } = initiatingEmployeeId;
}

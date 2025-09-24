using Shared.Dtos;

namespace Shared.Events;

public class TransferRequestCreatedEvent(TransferRequestDto transferRequest) : IEvent
{
    public TransferRequestDto TransferRequest { get; } = transferRequest;
}
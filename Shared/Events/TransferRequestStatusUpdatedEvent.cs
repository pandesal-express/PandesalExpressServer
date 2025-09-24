using Shared.Dtos;

namespace Shared.Events;

public class TransferRequestStatusUpdatedEvent(TransferRequestDto transferRequest) : IEvent
{
    public TransferRequestDto TransferRequest { get; } = transferRequest;
}

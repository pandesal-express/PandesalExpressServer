using Shared.Dtos;

namespace Shared.Events;

public class TransferMessageAddedEvent : IEvent
{
    public TransferMessageDto TransferMessage { get; }

    public TransferMessageAddedEvent(TransferMessageDto transferMessage)
    {
        TransferMessage = transferMessage;
    }
}
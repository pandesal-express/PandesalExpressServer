using Shared.Dtos;

namespace Shared.Events;

public record PdndStatusChangedEvent(
    PdndRequestDto Request,
    string PreviousStatus,
    string NewStatus,
    string ChangedBy,
    string? Notes,
    DateTime ChangedAt
) : IEvent;
using Shared.Dtos;

namespace Shared.Events;

public record PdndRequestEvent(PdndRequestDto PdndRequest) : IEvent;

namespace Siaed.Domain.Events;

public sealed record StudentUpdatedEvent(
    Guid StudentId,
    DateTime OccurredAt);

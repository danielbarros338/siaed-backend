namespace Siaed.Domain.Events;

public sealed record StudentCreatedEvent(
    Guid StudentId,
    string FullName,
    Guid ClassId,
    DateTime OccurredAt);

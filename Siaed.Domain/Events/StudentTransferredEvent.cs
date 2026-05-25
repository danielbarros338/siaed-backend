namespace Siaed.Domain.Events;

public sealed record StudentTransferredEvent(
    Guid StudentId,
    Guid FromClassId,
    Guid ToClassId,
    DateTime OccurredAt);

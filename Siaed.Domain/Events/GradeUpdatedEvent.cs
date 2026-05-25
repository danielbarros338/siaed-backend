namespace Siaed.Domain.Events;

public sealed record GradeUpdatedEvent(Guid GradeId, Guid ActivityId, Guid StudentId, Guid TeacherId, DateTime OccurredAtUtc);

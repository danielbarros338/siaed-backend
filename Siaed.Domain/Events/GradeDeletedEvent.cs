namespace Siaed.Domain.Events;

public sealed record GradeDeletedEvent(Guid GradeId, Guid ActivityId, Guid StudentId, Guid TeacherId, DateTime OccurredAtUtc);

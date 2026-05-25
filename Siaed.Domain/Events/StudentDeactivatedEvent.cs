using Siaed.Domain.Enums;

namespace Siaed.Domain.Events;

public sealed record StudentDeactivatedEvent(
    Guid StudentId,
    StudentStatus NewStatus,
    DateTime OccurredAt);

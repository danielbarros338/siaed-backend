namespace Siaed.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    protected void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;
    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}

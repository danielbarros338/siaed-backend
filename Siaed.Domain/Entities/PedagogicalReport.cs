namespace Siaed.Domain.Entities;

public sealed class PedagogicalReport : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string ParentCommunication { get; private set; } = string.Empty;
    public bool IsAIGenerated { get; private set; }

    private PedagogicalReport() { }

    public static PedagogicalReport Create(
        Guid userId,
        Guid studentId,
        string content,
        string summary = "",
        string parentCommunication = "",
        bool isAIGenerated = false)
    {
        return new PedagogicalReport
        {
            UserId = userId,
            StudentId = studentId,
            Content = content.Trim(),
            Summary = summary.Trim(),
            ParentCommunication = parentCommunication.Trim(),
            IsAIGenerated = isAIGenerated
        };
    }

    public void Update(
        Guid studentId,
        string content,
        string summary,
        string parentCommunication,
        bool isAIGenerated)
    {
        StudentId = studentId;
        Content = content.Trim();
        Summary = summary.Trim();
        ParentCommunication = parentCommunication.Trim();
        IsAIGenerated = isAIGenerated;
        MarkAsUpdated();
    }

    public void SetSummary(string summary)
    {
        Summary = summary.Trim();
        MarkAsUpdated();
    }

    public void SetParentCommunication(string communication)
    {
        ParentCommunication = communication.Trim();
        MarkAsUpdated();
    }

    public void Delete() => MarkAsDeleted();
}

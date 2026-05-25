using Siaed.Domain.Enums;

namespace Siaed.Domain.Entities;

public sealed class LessonPlan : BaseEntity
{
    public Guid TeacherId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public string Objectives { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Methodology { get; private set; } = string.Empty;
    public string Resources { get; private set; } = string.Empty;
    public string Evaluation { get; private set; } = string.Empty;
    public string AgeRange { get; private set; } = string.Empty;
    public bool IsAIGenerated { get; private set; }
    public LessonPlanStatus Status { get; private set; } = LessonPlanStatus.Draft;

    private LessonPlan() { }

    public static LessonPlan Create(
        Guid teacherId,
        string title,
        string subject,
        string grade,
        int durationMinutes,
        string objectives,
        string content,
        string methodology,
        string resources,
        string evaluation,
        string ageRange,
        bool isAIGenerated = false)
    {
        return new LessonPlan
        {
            TeacherId = teacherId,
            Title = title.Trim(),
            Subject = subject.Trim(),
            Grade = grade.Trim(),
            DurationMinutes = durationMinutes,
            Objectives = objectives.Trim(),
            Content = content.Trim(),
            Methodology = methodology.Trim(),
            Resources = resources.Trim(),
            Evaluation = evaluation.Trim(),
            AgeRange = ageRange.Trim(),
            IsAIGenerated = isAIGenerated
        };
    }

    public void Update(string title, string objectives, string content, string methodology, string resources, string evaluation)
    {
        Title = title.Trim();
        Objectives = objectives.Trim();
        Content = content.Trim();
        Methodology = methodology.Trim();
        Resources = resources.Trim();
        Evaluation = evaluation.Trim();
        MarkAsUpdated();
    }

    public void Publish()
    {
        Status = LessonPlanStatus.Published;
        MarkAsUpdated();
    }

    public void Archive()
    {
        Status = LessonPlanStatus.Archived;
        MarkAsUpdated();
    }

    public void Delete() => MarkAsDeleted();
}

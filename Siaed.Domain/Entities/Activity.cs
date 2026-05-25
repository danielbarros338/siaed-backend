using Siaed.Domain.Enums;

namespace Siaed.Domain.Entities;

public sealed class Activity : BaseEntity
{
    public Guid TeacherId { get; private set; }
    public Guid? LessonPlanId { get; private set; }
    public Guid? SchoolClassId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public string GradeConventionKey { get; private set; } = string.Empty;
    public string AgeRange { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string AnswerKey { get; private set; } = string.Empty;
    public string SimplifiedVersion { get; private set; } = string.Empty;
    public ActivityType Type { get; private set; }
    public bool IsAIGenerated { get; private set; }
    public ActivityStatus Status { get; private set; } = ActivityStatus.Draft;

    private Activity() { }

    public static Activity Create(
        Guid teacherId,
        string title,
        string description,
        string subject,
        string grade,
        string ageRange,
        string content,
        ActivityType type,
        Guid? lessonPlanId = null,
        bool isAIGenerated = false,
        Guid? schoolClassId = null,
        string? gradeConventionKey = null)
    {
        return new Activity
        {
            TeacherId = teacherId,
            LessonPlanId = lessonPlanId,
            SchoolClassId = schoolClassId,
            Title = title.Trim(),
            Description = description.Trim(),
            Subject = subject.Trim(),
            Grade = grade.Trim(),
            GradeConventionKey = gradeConventionKey?.Trim() ?? string.Empty,
            AgeRange = ageRange.Trim(),
            Content = content.Trim(),
            Type = type,
            IsAIGenerated = isAIGenerated
        };
    }

    public void Update(string title, string description, string content)
    {
        Title = title.Trim();
        Description = description.Trim();
        Content = content.Trim();
        MarkAsUpdated();
    }

    public void SetGradeContext(Guid schoolClassId, string gradeConventionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gradeConventionKey);

        SchoolClassId = schoolClassId;
        GradeConventionKey = gradeConventionKey.Trim();
        MarkAsUpdated();
    }

    public void SetAnswerKey(string answerKey)
    {
        AnswerKey = answerKey.Trim();
        MarkAsUpdated();
    }

    public void SetSimplifiedVersion(string simplifiedVersion)
    {
        SimplifiedVersion = simplifiedVersion.Trim();
        MarkAsUpdated();
    }

    public void Publish()
    {
        Status = ActivityStatus.Published;
        MarkAsUpdated();
    }

    public void Archive()
    {
        Status = ActivityStatus.Archived;
        MarkAsUpdated();
    }

    public void Delete() => MarkAsDeleted();
}

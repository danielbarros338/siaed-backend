namespace Siaed.Domain.Entities;

public sealed class Grade : BaseEntity
{
    public Guid ActivityId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid SchoolClassId { get; private set; }
    public Guid TeacherId { get; private set; }
    public string GradeValue { get; private set; } = string.Empty;
    public string ConventionKey { get; private set; } = string.Empty;
    public byte[] Version { get; private set; } = Array.Empty<byte>();

    private Grade() { }

    public static Grade Create(
        Guid activityId,
        Guid studentId,
        Guid schoolClassId,
        Guid teacherId,
        string gradeValue,
        string conventionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gradeValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(conventionKey);

        return new Grade
        {
            ActivityId = activityId,
            StudentId = studentId,
            SchoolClassId = schoolClassId,
            TeacherId = teacherId,
            GradeValue = gradeValue.Trim(),
            ConventionKey = conventionKey.Trim()
        };
    }

    public void UpdateValue(string gradeValue, string conventionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gradeValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(conventionKey);

        GradeValue = gradeValue.Trim();
        ConventionKey = conventionKey.Trim();
        MarkAsUpdated();
    }

    public void Delete()
    {
        MarkAsDeleted();
        MarkAsUpdated();
    }
}

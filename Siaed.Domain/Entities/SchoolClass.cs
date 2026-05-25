using Siaed.Domain.Enums;

namespace Siaed.Domain.Entities;

public sealed class SchoolClass : BaseEntity
{
    private readonly List<User> _teachers = [];

    public string Name { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public int SchoolYear { get; private set; }
    public ClassStatus Status { get; private set; }
    public IReadOnlyList<User> Teachers => _teachers.AsReadOnly();

    private SchoolClass() { }

    public static SchoolClass Create(string name, string grade, int schoolYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(grade);

        if (schoolYear < 2000 || schoolYear > 2100)
            throw new ArgumentOutOfRangeException(nameof(schoolYear), "Ano letivo inválido.");

        return new SchoolClass
        {
            Name = name.Trim(),
            Grade = grade.Trim(),
            SchoolYear = schoolYear,
            Status = ClassStatus.Active
        };
    }

    public void Update(string name, string grade, int schoolYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(grade);

        Name = name.Trim();
        Grade = grade.Trim();
        SchoolYear = schoolYear;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        Status = ClassStatus.Inactive;
        MarkAsUpdated();
    }

    public void Reactivate()
    {
        Status = ClassStatus.Active;
        MarkAsUpdated();
    }

    public void AssignTeacher(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Role != UserRole.Professor)
            throw new ArgumentException("Somente usuários com perfil de Professor podem ser associados a uma turma.");

        if (_teachers.All(t => t.Id != user.Id))
            _teachers.Add(user);
    }

    public void RemoveTeacher(Guid userId)
    {
        var teacher = _teachers.FirstOrDefault(t => t.Id == userId);
        if (teacher is not null)
            _teachers.Remove(teacher);
    }
}

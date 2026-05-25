using Siaed.Domain.Enums;

namespace Siaed.Domain.Entities;

public sealed class Student : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public string DocumentId { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public Guid ClassId { get; private set; }
    public StudentStatus Status { get; private set; }
    public DateOnly EnrollmentDate { get; private set; }
    public string? Notes { get; private set; }

    private Student() { }

    public static Student Create(
        string fullName,
        DocumentType documentType,
        string documentId,
        DateOnly birthDate,
        Guid classId,
        DateOnly enrollmentDate,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        return new Student
        {
            FullName = fullName.Trim(),
            DocumentType = documentType,
            DocumentId = documentId.Trim(),
            BirthDate = birthDate,
            ClassId = classId,
            Status = StudentStatus.Ativo,
            EnrollmentDate = enrollmentDate,
            Notes = notes?.Trim()
        };
    }

    public void Update(
        string fullName,
        DocumentType documentType,
        string documentId,
        DateOnly birthDate,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        FullName = fullName.Trim();
        DocumentType = documentType;
        DocumentId = documentId.Trim();
        BirthDate = birthDate;
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    public void Transfer(Guid newClassId)
    {
        if (Status != StudentStatus.Ativo)
            throw new InvalidOperationException("Apenas alunos ativos podem ser transferidos.");

        ClassId = newClassId;
        MarkAsUpdated();
    }

    public void Deactivate(StudentStatus newStatus)
    {
        if (newStatus is not (StudentStatus.Inativo or StudentStatus.Evadido))
            throw new ArgumentException("Status de inativação inválido. Use Inativo ou Evadido.", nameof(newStatus));

        Status = newStatus;
        MarkAsUpdated();
    }

    public void Reactivate(Guid classId)
    {
        Status = StudentStatus.Ativo;
        ClassId = classId;
        MarkAsUpdated();
    }
}

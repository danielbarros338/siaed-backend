using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Students.DTOs;

public sealed record StudentDto(
    Guid Id,
    string FullName,
    DocumentType DocumentType,
    string DocumentIdMasked,
    DateOnly BirthDate,
    Guid ClassId,
    string ClassName,
    StudentStatus Status,
    DateOnly EnrollmentDate,
    string? Notes,
    DateTime CreatedAt
);

public sealed record StudentSummaryDto(
    Guid Id,
    string FullName,
    string DocumentIdMasked,
    Guid ClassId,
    string ClassName,
    StudentStatus Status
);

public static class DocumentIdMasker
{
    public static string Mask(string documentId)
        => documentId.Length >= 7
            ? documentId[..3] + "***" + documentId[^4..]
            : "***";
}

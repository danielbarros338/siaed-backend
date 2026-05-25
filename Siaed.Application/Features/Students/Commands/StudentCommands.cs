using MediatR;
using Siaed.Application.Common;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Students.Commands;

public sealed record RegisterStudentCommand(
    string FullName,
    DocumentType DocumentType,
    string DocumentId,
    DateOnly BirthDate,
    Guid ClassId,
    DateOnly EnrollmentDate,
    string? Notes
) : IRequest<Result<Guid>>;

public sealed record UpdateStudentCommand(
    Guid Id,
    string FullName,
    DocumentType DocumentType,
    string DocumentId,
    DateOnly BirthDate,
    string? Notes
) : IRequest<Result>;

public sealed record TransferStudentCommand(
    Guid StudentId,
    Guid NewClassId
) : IRequest<Result>;

public sealed record DeactivateStudentCommand(
    Guid StudentId,
    StudentStatus NewStatus
) : IRequest<Result>;

public sealed record ReactivateStudentCommand(
    Guid StudentId,
    Guid ClassId
) : IRequest<Result>;

public sealed record ImportStudentsFromCsvCommand(
    byte[] FileContent
) : IRequest<Result<ImportSummaryDto>>;

public sealed record ImportSummaryDto(int Imported, int Skipped, IReadOnlyList<string> Errors);

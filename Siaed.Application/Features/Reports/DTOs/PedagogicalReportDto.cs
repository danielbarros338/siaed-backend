namespace Siaed.Application.Features.Reports.DTOs;

public sealed record PedagogicalReportDto(
    Guid Id,
    Guid UserId,
    Guid StudentId,
    string Content,
    string Summary,
    string ParentCommunication,
    bool IsAIGenerated,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ReportUserDto User,
    ReportStudentDto Student);

public sealed record ReportUserDto(
    Guid Id,
    string Name,
    string Email);

public sealed record ReportStudentDto(
    Guid Id,
    string FullName,
    Guid ClassId,
    DateOnly EnrollmentDate,
    string? Notes);

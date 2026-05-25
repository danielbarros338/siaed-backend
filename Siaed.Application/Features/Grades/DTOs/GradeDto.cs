namespace Siaed.Application.Features.Grades.DTOs;

public sealed record GradeDto(
    Guid Id,
    Guid ActivityId,
    Guid StudentId,
    Guid SchoolClassId,
    Guid TeacherId,
    string GradeValue,
    string ConventionKey,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Version
);

public sealed record GradeListItemDto(
    Guid Id,
    Guid ActivityId,
    Guid StudentId,
    string StudentName,
    Guid SchoolClassId,
    Guid TeacherId,
    string GradeValue,
    string ConventionKey,
    DateTime UpdatedAt
);

public sealed record ActivityGradesDto(
    Guid ActivityId,
    Guid SchoolClassId,
    IReadOnlyList<ActivityGradeItemDto> Items
);

public sealed record ActivityGradeItemDto(
    Guid StudentId,
    string StudentName,
    Guid? GradeId,
    string? GradeValue,
    bool HasGrade,
    bool IsHistorical
);

using Siaed.Application.Features.Teachers.DTOs;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.SchoolClasses.DTOs;

public sealed record SchoolClassDto(
    Guid Id,
    string Name,
    string Grade,
    int SchoolYear,
    ClassStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<TeacherDto> Teachers
);

public sealed record SchoolClassSummaryDto(
    Guid Id,
    string Name,
    string Grade,
    int SchoolYear,
    ClassStatus Status
);

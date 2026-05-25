using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Grades.DTOs;

namespace Siaed.Application.Features.Grades.Commands;

public sealed record CreateGradeCommand(
    Guid ActivityId,
    Guid StudentId,
    Guid SchoolClassId,
    Guid TeacherId,
    string GradeValue,
    string ConventionKey,
    Guid RequestingUserId
) : IRequest<Result<Guid>>;

public sealed record UpdateGradeCommand(
    Guid Id,
    string GradeValue,
    string ConventionKey,
    string Version,
    Guid RequestingUserId
) : IRequest<Result<GradeDto>>;

public sealed record DeleteGradeCommand(
    Guid Id,
    Guid RequestingUserId
) : IRequest<Result>;

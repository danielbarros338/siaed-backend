using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Grades.DTOs;

namespace Siaed.Application.Features.Grades.Queries;

public sealed record GetGradeByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<GradeDto>>;

public sealed record ListGradesQuery(
    int Page,
    int PageSize,
    Guid? ActivityId,
    Guid? SchoolClassId,
    Guid? TeacherId,
    string? GradeValue,
    Guid RequestingUserId
) : IRequest<Result<PagedResult<GradeListItemDto>>>;

public sealed record GetActivityGradesQuery(Guid ActivityId, Guid RequestingUserId) : IRequest<Result<ActivityGradesDto>>;

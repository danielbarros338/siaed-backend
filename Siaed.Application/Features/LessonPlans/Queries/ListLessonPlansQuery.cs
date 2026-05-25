using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.DTOs;

namespace Siaed.Application.Features.LessonPlans.Queries;

public sealed record ListLessonPlansQuery(
    Guid TeacherId,
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    bool? IsAIGenerated = null) : IRequest<Result<PagedResult<LessonPlanDto>>>;

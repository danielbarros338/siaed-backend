using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.DTOs;

namespace Siaed.Application.Features.Activities.Queries;

public sealed record ListActivitiesQuery(
    Guid TeacherId,
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    bool? IsAIGenerated = null) : IRequest<Result<PagedResult<ActivityDto>>>;

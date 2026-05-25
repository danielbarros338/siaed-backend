using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.DTOs;
using Siaed.Application.Features.Activities.Queries;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class ListActivitiesQueryHandler
    : IRequestHandler<ListActivitiesQuery, Result<PagedResult<ActivityDto>>>
{
    private readonly IActivityRepository _repository;

    public ListActivitiesQueryHandler(IActivityRepository repository)
        => _repository = repository;

    public async Task<Result<PagedResult<ActivityDto>>> Handle(ListActivitiesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetByTeacherIdAsync(request.TeacherId, request.Page, request.PageSize, request.Status, request.IsAIGenerated, ct);

        var dtos = items.Select(a => new ActivityDto(
            a.Id, a.TeacherId, a.LessonPlanId, a.Title, a.Description,
            a.Subject, a.Grade, a.AgeRange, a.Content, a.AnswerKey,
            a.SimplifiedVersion, a.Type, a.IsAIGenerated, a.Status,
            a.CreatedAt, a.UpdatedAt)).ToList();

        return Result.Success(new PagedResult<ActivityDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.DTOs;
using Siaed.Application.Features.LessonPlans.Queries;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class ListLessonPlansQueryHandler
    : IRequestHandler<ListLessonPlansQuery, Result<PagedResult<LessonPlanDto>>>
{
    private readonly ILessonPlanRepository _repository;

    public ListLessonPlansQueryHandler(ILessonPlanRepository repository)
        => _repository = repository;

    public async Task<Result<PagedResult<LessonPlanDto>>> Handle(ListLessonPlansQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetByTeacherIdAsync(request.TeacherId, request.Page, request.PageSize, request.Status, request.IsAIGenerated, ct);

        var dtos = items.Select(lp => new LessonPlanDto(
            lp.Id, lp.TeacherId, lp.Title, lp.Subject, lp.Grade,
            lp.DurationMinutes, lp.Objectives, lp.Content, lp.Methodology,
            lp.Resources, lp.Evaluation, lp.AgeRange, lp.IsAIGenerated,
            lp.Status, lp.CreatedAt, lp.UpdatedAt)).ToList();

        var result = new PagedResult<LessonPlanDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result.Success(result);
    }
}

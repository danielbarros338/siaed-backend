using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.DTOs;
using Siaed.Application.Features.LessonPlans.Queries;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class GetLessonPlanByIdQueryHandler
    : IRequestHandler<GetLessonPlanByIdQuery, Result<LessonPlanDto>>
{
    private readonly ILessonPlanRepository _repository;

    public GetLessonPlanByIdQueryHandler(ILessonPlanRepository repository)
        => _repository = repository;

    public async Task<Result<LessonPlanDto>> Handle(GetLessonPlanByIdQuery request, CancellationToken ct)
    {
        var lessonPlan = await _repository.GetByIdAsync(request.Id, ct);
        if (lessonPlan is null || lessonPlan.TeacherId != request.RequestingUserId)
            return Result.Failure<LessonPlanDto>("Recurso não encontrado.");

        var dto = new LessonPlanDto(
            lessonPlan.Id,
            lessonPlan.TeacherId,
            lessonPlan.Title,
            lessonPlan.Subject,
            lessonPlan.Grade,
            lessonPlan.DurationMinutes,
            lessonPlan.Objectives,
            lessonPlan.Content,
            lessonPlan.Methodology,
            lessonPlan.Resources,
            lessonPlan.Evaluation,
            lessonPlan.AgeRange,
            lessonPlan.IsAIGenerated,
            lessonPlan.Status,
            lessonPlan.CreatedAt,
            lessonPlan.UpdatedAt);

        return Result.Success(dto);
    }
}

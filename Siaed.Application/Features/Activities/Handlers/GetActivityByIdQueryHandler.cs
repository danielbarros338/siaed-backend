using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.DTOs;
using Siaed.Application.Features.Activities.Queries;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class GetActivityByIdQueryHandler
    : IRequestHandler<GetActivityByIdQuery, Result<ActivityDto>>
{
    private readonly IActivityRepository _repository;

    public GetActivityByIdQueryHandler(IActivityRepository repository)
        => _repository = repository;

    public async Task<Result<ActivityDto>> Handle(GetActivityByIdQuery request, CancellationToken ct)
    {
        var activity = await _repository.GetByIdAsync(request.Id, ct);
        if (activity is null || activity.TeacherId != request.RequestingUserId)
            return Result.Failure<ActivityDto>("Recurso não encontrado.");

        var dto = new ActivityDto(
            activity.Id, activity.TeacherId, activity.LessonPlanId,
            activity.Title, activity.Description, activity.Subject,
            activity.Grade, activity.AgeRange, activity.Content,
            activity.AnswerKey, activity.SimplifiedVersion, activity.Type,
            activity.IsAIGenerated, activity.Status, activity.CreatedAt, activity.UpdatedAt);

        return Result.Success(dto);
    }
}

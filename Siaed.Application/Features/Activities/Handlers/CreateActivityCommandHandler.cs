using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class CreateActivityCommandHandler
    : IRequestHandler<CreateActivityCommand, Result<Guid>>
{
    private readonly IActivityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateActivityCommandHandler(IActivityRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateActivityCommand request, CancellationToken ct)
    {
        var activity = Activity.Create(
            request.TeacherId,
            request.Title,
            request.Description,
            request.Subject,
            request.Grade,
            request.AgeRange,
            request.Content,
            request.Type,
            request.LessonPlanId,
            schoolClassId: request.SchoolClassId,
            gradeConventionKey: request.GradeConventionKey);

        await _repository.AddAsync(activity, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(activity.Id);
    }
}

using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class CreateLessonPlanCommandHandler
    : IRequestHandler<CreateLessonPlanCommand, Result<Guid>>
{
    private readonly ILessonPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLessonPlanCommandHandler(ILessonPlanRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateLessonPlanCommand request, CancellationToken ct)
    {
        var lessonPlan = LessonPlan.Create(
            request.TeacherId,
            request.Title,
            request.Subject,
            request.Grade,
            request.DurationMinutes,
            request.Objectives,
            request.Content,
            request.Methodology,
            request.Resources,
            request.Evaluation,
            request.AgeRange);

        await _repository.AddAsync(lessonPlan, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(lessonPlan.Id);
    }
}

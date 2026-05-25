using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.Commands;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class UpdateLessonPlanCommandHandler
    : IRequestHandler<UpdateLessonPlanCommand, Result>
{
    private readonly ILessonPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLessonPlanCommandHandler(
        ILessonPlanRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateLessonPlanCommand request, CancellationToken ct)
    {
        var lessonPlan = await _repository.GetByIdAsync(request.Id, ct);
        if (lessonPlan is null || lessonPlan.TeacherId != request.RequestingUserId)
            return Result.Failure("Recurso não encontrado.");

        lessonPlan.Update(request.Title, request.Objectives, request.Content, request.Methodology, request.Resources, request.Evaluation);
        _repository.Update(lessonPlan);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}

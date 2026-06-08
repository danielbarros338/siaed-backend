using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.LessonPlans.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.LessonPlans.Handlers;

public sealed class ArchiveLessonPlanCommandHandler
    : IRequestHandler<ArchiveLessonPlanCommand, Result>
{
    private readonly ILessonPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveLessonPlanCommandHandler(
        ILessonPlanRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ArchiveLessonPlanCommand request, CancellationToken ct)
    {
        var lessonPlan = await _repository.GetByIdAsync(request.Id, ct);
        if (lessonPlan is null || lessonPlan.TeacherId != request.RequestingUserId)
            return Result.Failure("Recurso não encontrado.");

        try
        {
            lessonPlan.Archive();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _repository.Update(lessonPlan);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}

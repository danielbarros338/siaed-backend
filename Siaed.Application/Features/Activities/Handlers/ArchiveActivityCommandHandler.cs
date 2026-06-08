using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class ArchiveActivityCommandHandler
    : IRequestHandler<ArchiveActivityCommand, Result>
{
    private readonly IActivityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveActivityCommandHandler(
        IActivityRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ArchiveActivityCommand request, CancellationToken ct)
    {
        var activity = await _repository.GetByIdAsync(request.Id, ct);
        if (activity is null || activity.TeacherId != request.RequestingUserId)
            return Result.Failure("Recurso não encontrado.");

        try
        {
            activity.Archive();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _repository.Update(activity);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}

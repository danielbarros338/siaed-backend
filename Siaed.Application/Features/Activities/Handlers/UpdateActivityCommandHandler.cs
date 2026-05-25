using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.Commands;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Activities.Handlers;

public sealed class UpdateActivityCommandHandler
    : IRequestHandler<UpdateActivityCommand, Result>
{
    private readonly IActivityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateActivityCommandHandler(
        IActivityRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateActivityCommand request, CancellationToken ct)
    {
        var activity = await _repository.GetByIdAsync(request.Id, ct);
        if (activity is null || activity.TeacherId != request.RequestingUserId)
            return Result.Failure("Recurso não encontrado.");

        activity.Update(request.Title, request.Description, request.Content);
        _repository.Update(activity);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}

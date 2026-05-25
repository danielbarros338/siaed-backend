using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class DeleteReportCommandHandler
    : IRequestHandler<DeleteReportCommand, Result>
{
    private readonly IPedagogicalReportRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReportCommandHandler(
        IPedagogicalReportRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteReportCommand request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report is null || report.UserId != request.RequestingUserId)
            return Result.Failure("Recurso não encontrado.");

        report.Delete();
        _repository.Update(report);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }
}

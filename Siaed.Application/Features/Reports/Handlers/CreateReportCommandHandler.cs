using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class CreateReportCommandHandler
    : IRequestHandler<CreateReportCommand, Result<Guid>>
{
    private readonly IPedagogicalReportRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReportCommandHandler(IPedagogicalReportRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateReportCommand request, CancellationToken ct)
    {
        var report = PedagogicalReport.Create(
            request.UserId,
            request.StudentId,
            request.Content,
            request.Summary,
            request.ParentCommunication,
            request.IsAIGenerated);

        await _repository.AddAsync(report, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(report.Id);
    }
}

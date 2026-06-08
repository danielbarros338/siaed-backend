using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.DTOs;
using Siaed.Application.Features.Reports.Queries;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class GetReportByIdQueryHandler
    : IRequestHandler<GetReportByIdQuery, Result<PedagogicalReportDto>>
{
    private readonly IPedagogicalReportRepository _repository;

    public GetReportByIdQueryHandler(IPedagogicalReportRepository repository)
        => _repository = repository;

    public async Task<Result<PedagogicalReportDto>> Handle(GetReportByIdQuery request, CancellationToken ct)
    {
        var report = await _repository.GetByIdAsync(request.Id, ct);
        if (report is null || report.UserId != request.RequestingUserId)
            return Result.Failure<PedagogicalReportDto>("Recurso não encontrado.");

        var dto = new PedagogicalReportDto(
            report.Id,
            report.UserId,
            report.StudentId,
            report.Content,
            report.Summary,
            report.ParentCommunication,
            report.IsAIGenerated,
            report.CreatedAt,
            report.UpdatedAt,
            new ReportUserDto(
                report.User.Id,
                report.User.Name,
                report.User.Email),
            new ReportStudentDto(
                report.Student.Id,
                report.Student.FullName,
                report.Student.ClassId,
                report.Student.EnrollmentDate,
                report.Student.Notes));

        return Result.Success(dto);
    }
}

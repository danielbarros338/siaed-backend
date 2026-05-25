using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.DTOs;
using Siaed.Application.Features.Reports.Queries;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Reports.Handlers;

public sealed class ListReportsQueryHandler
    : IRequestHandler<ListReportsQuery, Result<PagedResult<PedagogicalReportDto>>>
{
    private readonly IPedagogicalReportRepository _repository;

    public ListReportsQueryHandler(IPedagogicalReportRepository repository)
        => _repository = repository;

    public async Task<Result<PagedResult<PedagogicalReportDto>>> Handle(ListReportsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, request.IsAIGenerated, ct);

        var dtos = items.Select(r => new PedagogicalReportDto(
            r.Id,
            r.UserId,
            r.StudentId,
            r.Content,
            r.Summary,
            r.ParentCommunication,
            r.IsAIGenerated,
            r.CreatedAt,
            r.UpdatedAt,
            new ReportUserDto(
                r.User.Id,
                r.User.Name,
                r.User.Email),
            new ReportStudentDto(
                r.Student.Id,
                r.Student.FullName,
                r.Student.ClassId,
                r.Student.EnrollmentDate,
                r.Student.Notes))).ToList();

        return Result.Success(new PagedResult<PedagogicalReportDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

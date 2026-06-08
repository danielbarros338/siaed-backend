using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.SchoolClasses.DTOs;
using Siaed.Application.Features.SchoolClasses.Queries;
using Siaed.Application.Features.Teachers.DTOs;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.SchoolClasses.Handlers;

public sealed class GetSchoolClassByIdQueryHandler : IRequestHandler<GetSchoolClassByIdQuery, Result<SchoolClassDto>>
{
    private readonly ISchoolClassRepository _repository;

    public GetSchoolClassByIdQueryHandler(ISchoolClassRepository repository) => _repository = repository;

    public async Task<Result<SchoolClassDto>> Handle(GetSchoolClassByIdQuery request, CancellationToken cancellationToken)
    {
        var schoolClass = await _repository.GetByIdWithTeachersAsync(request.Id, cancellationToken);
        if (schoolClass is null)
            return Result<SchoolClassDto>.Failure("Turma não encontrada.");

        var teachers = schoolClass.Teachers
            .Select(u => new TeacherDto(u.Id, u.Name, u.Email, u.CreatedAt))
            .ToList();

        return Result<SchoolClassDto>.Success(new SchoolClassDto(
            schoolClass.Id,
            schoolClass.Name,
            schoolClass.Grade,
            schoolClass.SchoolYear,
            schoolClass.Status,
            schoolClass.CreatedAt,
            teachers
        ));
    }
}

public sealed class GetSchoolClassesPagedQueryHandler : IRequestHandler<GetSchoolClassesPagedQuery, Result<PagedResult<SchoolClassSummaryDto>>>
{
    private readonly ISchoolClassRepository _repository;

    public GetSchoolClassesPagedQueryHandler(ISchoolClassRepository repository) => _repository = repository;

    public async Task<Result<PagedResult<SchoolClassSummaryDto>>> Handle(GetSchoolClassesPagedQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repository.ListAsync(request.Page, request.PageSize, request.Search, cancellationToken);

        var dtos = paged.Items.Select(t => new SchoolClassSummaryDto(
            t.Id, t.Name, t.Grade, t.SchoolYear, t.Status
        )).ToList();

        return Result<PagedResult<SchoolClassSummaryDto>>.Success(new PagedResult<SchoolClassSummaryDto>
        {
            Items = dtos,
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }
}

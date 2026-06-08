using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Students.DTOs;
using Siaed.Application.Features.Students.Queries;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.Students.Handlers;

public sealed class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;

    public GetStudentByIdQueryHandler(IStudentRepository studentRepository, ISchoolClassRepository classRepository)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
    }

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null)
            return Result<StudentDto>.Failure("Aluno não encontrado.");

        var schoolClass = await _classRepository.GetByIdAsync(student.ClassId, cancellationToken);
        var className = schoolClass?.Name ?? string.Empty;

        var dto = new StudentDto(
            student.Id,
            student.FullName,
            student.DocumentType,
            DocumentIdMasker.Mask(student.DocumentId),
            student.BirthDate,
            student.ClassId,
            className,
            student.Status,
            student.EnrollmentDate,
            student.Notes,
            student.CreatedAt);

        return Result<StudentDto>.Success(dto);
    }
}

public sealed class GetStudentsPagedQueryHandler : IRequestHandler<GetStudentsPagedQuery, Result<PagedResult<StudentSummaryDto>>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;

    public GetStudentsPagedQueryHandler(IStudentRepository studentRepository, ISchoolClassRepository classRepository)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
    }

    public async Task<Result<PagedResult<StudentSummaryDto>>> Handle(GetStudentsPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedStudents = await _studentRepository.ListAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Status,
            request.ClassId,
            cancellationToken);

        var classIds = pagedStudents.Items.Select(s => s.ClassId).Distinct().ToList();
        var classNames = new Dictionary<Guid, string>();
        foreach (var classId in classIds)
        {
            var schoolClass = await _classRepository.GetByIdAsync(classId, cancellationToken);
            if (schoolClass is not null)
                classNames[classId] = schoolClass.Name;
        }

        var dtos = pagedStudents.Items.Select(s => new StudentSummaryDto(
            s.Id,
            s.FullName,
            DocumentIdMasker.Mask(s.DocumentId),
            s.ClassId,
            classNames.GetValueOrDefault(s.ClassId, string.Empty),
            s.Status)).ToList();

        return Result<PagedResult<StudentSummaryDto>>.Success(
            new PagedResult<StudentSummaryDto>
            {
                Items = dtos,
                TotalCount = pagedStudents.TotalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
    }
}

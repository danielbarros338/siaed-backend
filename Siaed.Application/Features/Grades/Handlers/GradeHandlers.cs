using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Grades.Commands;
using Siaed.Application.Features.Grades.DTOs;
using Siaed.Application.Features.Grades.Queries;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Grades.Handlers;

public sealed class CreateGradeHandler : IRequestHandler<CreateGradeCommand, Result<Guid>>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGradeHandler(
        IGradeRepository gradeRepository,
        IActivityRepository activityRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _gradeRepository = gradeRepository;
        _activityRepository = activityRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateGradeCommand request, CancellationToken ct)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, ct);
        if (activity is null)
            return Result.Failure<Guid>("Atividade não encontrada.");

        if (activity.Status == ActivityStatus.Archived)
            return Result.Failure<Guid>("A atividade está fechada para edição de notas.");

        var authResult = await AuthorizeWriteAsync(request.RequestingUserId, activity, ct);
        if (authResult.IsFailure)
            return Result<Guid>.Failure(authResult.Errors);

        var student = await _studentRepository.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result.Failure<Guid>("Aluno não encontrado.");

        if (student.ClassId != request.SchoolClassId)
            return Result.Failure<Guid>("O aluno não pertence à turma informada.");

        var existing = await _gradeRepository.GetActiveByActivityAndStudentAsync(request.ActivityId, request.StudentId, ct);
        if (existing is not null)
            return Result.Failure<Guid>("Já existe nota cadastrada para este aluno na atividade.");

        var conventionResult = ValidateConvention(request.ConventionKey, request.GradeValue);
        if (conventionResult.IsFailure)
            return Result<Guid>.Failure(conventionResult.Errors);

        var grade = Grade.Create(
            request.ActivityId,
            request.StudentId,
            request.SchoolClassId,
            request.TeacherId,
            request.GradeValue,
            request.ConventionKey);

        await _gradeRepository.AddAsync(grade, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(grade.Id);
    }

    private async Task<Result> AuthorizeWriteAsync(Guid requestingUserId, Activity activity, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(requestingUserId, ct);
        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        if (user.Role == UserRole.Coordenador)
            return Result.Success();

        if (user.Role == UserRole.Professor && activity.TeacherId == requestingUserId)
            return Result.Success();

        return Result.Failure("Usuário sem permissão para lançar nota nesta atividade.");
    }

    private static Result ValidateConvention(string conventionKey, string gradeValue)
    {
        if (string.IsNullOrWhiteSpace(gradeValue))
            return Result.Failure("O valor da nota é obrigatório.");

        if (string.IsNullOrWhiteSpace(conventionKey))
            return Result.Failure("A convenção de nota é obrigatória.");

        var parts = conventionKey.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            var allowed = parts[1]
                .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowed.Count > 0 && !allowed.Contains(gradeValue.Trim()))
                return Result.Failure("O valor da nota não pertence à convenção permitida da atividade.");
        }

        return Result.Success();
    }
}

public sealed class UpdateGradeHandler : IRequestHandler<UpdateGradeCommand, Result<GradeDto>>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGradeHandler(
        IGradeRepository gradeRepository,
        IActivityRepository activityRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _gradeRepository = gradeRepository;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GradeDto>> Handle(UpdateGradeCommand request, CancellationToken ct)
    {
        var grade = await _gradeRepository.GetByIdAsync(request.Id, ct);
        if (grade is null)
            return Result.Failure<GradeDto>("Nota não encontrada.");

        var activity = await _activityRepository.GetByIdAsync(grade.ActivityId, ct);
        if (activity is null)
            return Result.Failure<GradeDto>("Atividade não encontrada.");

        if (activity.Status == ActivityStatus.Archived)
            return Result.Failure<GradeDto>("A atividade está fechada para edição de notas.");

        var authResult = await AuthorizeWriteAsync(request.RequestingUserId, activity, ct);
        if (authResult.IsFailure)
            return Result<GradeDto>.Failure(authResult.Errors);

        var currentVersion = Convert.ToBase64String(grade.Version);
        if (!string.Equals(currentVersion, request.Version, StringComparison.Ordinal))
            return Result.Failure<GradeDto>("Conflito de concorrencia detectado. Recarregue a nota antes de salvar novamente.");

        var conventionResult = ValidateConvention(request.ConventionKey, request.GradeValue);
        if (conventionResult.IsFailure)
            return Result<GradeDto>.Failure(conventionResult.Errors);

        grade.UpdateValue(request.GradeValue, request.ConventionKey);
        _gradeRepository.Update(grade);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(new GradeDto(
            grade.Id,
            grade.ActivityId,
            grade.StudentId,
            grade.SchoolClassId,
            grade.TeacherId,
            grade.GradeValue,
            grade.ConventionKey,
            grade.CreatedAt,
            grade.UpdatedAt,
            Convert.ToBase64String(grade.Version)));
    }

    private async Task<Result> AuthorizeWriteAsync(Guid requestingUserId, Activity activity, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(requestingUserId, ct);
        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        if (user.Role == UserRole.Coordenador)
            return Result.Success();

        if (user.Role == UserRole.Professor && activity.TeacherId == requestingUserId)
            return Result.Success();

        return Result.Failure("Usuário sem permissão para editar nota nesta atividade.");
    }

    private static Result ValidateConvention(string conventionKey, string gradeValue)
    {
        if (string.IsNullOrWhiteSpace(gradeValue))
            return Result.Failure("O valor da nota é obrigatório.");

        if (string.IsNullOrWhiteSpace(conventionKey))
            return Result.Failure("A convenção de nota é obrigatória.");

        var parts = conventionKey.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            var allowed = parts[1]
                .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowed.Count > 0 && !allowed.Contains(gradeValue.Trim()))
                return Result.Failure("O valor da nota não pertence à convenção permitida da atividade.");
        }

        return Result.Success();
    }
}

public sealed class DeleteGradeHandler : IRequestHandler<DeleteGradeCommand, Result>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteGradeHandler(
        IGradeRepository gradeRepository,
        IActivityRepository activityRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _gradeRepository = gradeRepository;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteGradeCommand request, CancellationToken ct)
    {
        var grade = await _gradeRepository.GetByIdAsync(request.Id, ct);
        if (grade is null)
            return Result.Failure("Nota não encontrada.");

        var activity = await _activityRepository.GetByIdAsync(grade.ActivityId, ct);
        if (activity is null)
            return Result.Failure("Atividade não encontrada.");

        var authResult = await AuthorizeWriteAsync(request.RequestingUserId, activity, ct);
        if (authResult.IsFailure)
            return authResult;

        grade.Delete();
        _gradeRepository.Update(grade);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success();
    }

    private async Task<Result> AuthorizeWriteAsync(Guid requestingUserId, Activity activity, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(requestingUserId, ct);
        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        if (user.Role == UserRole.Coordenador)
            return Result.Success();

        if (user.Role == UserRole.Professor && activity.TeacherId == requestingUserId)
            return Result.Success();

        return Result.Failure("Usuário sem permissão para excluir nota nesta atividade.");
    }
}

public sealed class GetGradeByIdHandler : IRequestHandler<GetGradeByIdQuery, Result<GradeDto>>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUserRepository _userRepository;

    public GetGradeByIdHandler(
        IGradeRepository gradeRepository,
        IActivityRepository activityRepository,
        IUserRepository userRepository)
    {
        _gradeRepository = gradeRepository;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<GradeDto>> Handle(GetGradeByIdQuery request, CancellationToken ct)
    {
        var grade = await _gradeRepository.GetByIdAsync(request.Id, ct);
        if (grade is null)
            return Result.Failure<GradeDto>("Nota não encontrada.");

        var activity = await _activityRepository.GetByIdAsync(grade.ActivityId, ct);
        if (activity is null)
            return Result.Failure<GradeDto>("Atividade não encontrada.");

        var authResult = await AuthorizeReadAsync(request.RequestingUserId, activity, ct);
        if (authResult.IsFailure)
            return Result<GradeDto>.Failure(authResult.Errors);

        return Result.Success(new GradeDto(
            grade.Id,
            grade.ActivityId,
            grade.StudentId,
            grade.SchoolClassId,
            grade.TeacherId,
            grade.GradeValue,
            grade.ConventionKey,
            grade.CreatedAt,
            grade.UpdatedAt,
            Convert.ToBase64String(grade.Version)));
    }

    private async Task<Result> AuthorizeReadAsync(Guid requestingUserId, Activity activity, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(requestingUserId, ct);
        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        if (user.Role is UserRole.Coordenador or UserRole.Diretor)
            return Result.Success();

        if (user.Role == UserRole.Professor && activity.TeacherId == requestingUserId)
            return Result.Success();

        return Result.Failure("Usuário sem permissão para consultar nota nesta atividade.");
    }
}

public sealed class ListGradesHandler : IRequestHandler<ListGradesQuery, Result<PagedResult<GradeListItemDto>>>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;

    public ListGradesHandler(
        IGradeRepository gradeRepository,
        IUserRepository userRepository,
        IStudentRepository studentRepository)
    {
        _gradeRepository = gradeRepository;
        _userRepository = userRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<PagedResult<GradeListItemDto>>> Handle(ListGradesQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.RequestingUserId, ct);
        if (user is null)
            return Result.Failure<PagedResult<GradeListItemDto>>("Usuário não encontrado.");

        var teacherFilter = request.TeacherId;
        if (user.Role == UserRole.Professor)
            teacherFilter = request.RequestingUserId;

        if (user.Role is not (UserRole.Professor or UserRole.Coordenador or UserRole.Diretor))
            return Result.Failure<PagedResult<GradeListItemDto>>("Usuário sem permissão para consultar notas.");

        var paged = await _gradeRepository.ListAsync(
            request.Page,
            request.PageSize,
            request.ActivityId,
            request.SchoolClassId,
            teacherFilter,
            request.GradeValue,
            ct);

        var items = new List<GradeListItemDto>(paged.Items.Count);

        foreach (var grade in paged.Items)
        {
            var student = await _studentRepository.GetByIdAsync(grade.StudentId, ct);
            items.Add(new GradeListItemDto(
                grade.Id,
                grade.ActivityId,
                grade.StudentId,
                student?.FullName ?? "Aluno não encontrado",
                grade.SchoolClassId,
                grade.TeacherId,
                grade.GradeValue,
                grade.ConventionKey,
                grade.UpdatedAt));
        }

        return Result.Success(new PagedResult<GradeListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }
}

public sealed class GetActivityGradesHandler : IRequestHandler<GetActivityGradesQuery, Result<ActivityGradesDto>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IGradeRepository _gradeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;

    public GetActivityGradesHandler(
        IActivityRepository activityRepository,
        IGradeRepository gradeRepository,
        IUserRepository userRepository,
        IStudentRepository studentRepository)
    {
        _activityRepository = activityRepository;
        _gradeRepository = gradeRepository;
        _userRepository = userRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<ActivityGradesDto>> Handle(GetActivityGradesQuery request, CancellationToken ct)
    {
        var activity = await _activityRepository.GetByIdAsync(request.ActivityId, ct);
        if (activity is null)
            return Result.Failure<ActivityGradesDto>("Atividade não encontrada.");

        if (!activity.SchoolClassId.HasValue)
            return Result.Failure<ActivityGradesDto>("A atividade não possui turma vinculada para consulta de notas.");

        var authResult = await AuthorizeReadAsync(request.RequestingUserId, activity, ct);
        if (authResult.IsFailure)
            return Result<ActivityGradesDto>.Failure(authResult.Errors);

        var grades = await _gradeRepository.ListByActivityIdAsync(request.ActivityId, ct);
        var currentStudents = await _studentRepository.ListByClassIdAsync(activity.SchoolClassId.Value, ct);

        var gradeByStudent = grades.ToDictionary(g => g.StudentId, g => g);
        var items = new List<ActivityGradeItemDto>();

        foreach (var student in currentStudents)
        {
            if (gradeByStudent.TryGetValue(student.Id, out var grade))
            {
                items.Add(new ActivityGradeItemDto(student.Id, student.FullName, grade.Id, grade.GradeValue, true, false));
                gradeByStudent.Remove(student.Id);
            }
            else
            {
                items.Add(new ActivityGradeItemDto(student.Id, student.FullName, null, null, false, false));
            }
        }

        foreach (var historical in gradeByStudent.Values)
        {
            var student = await _studentRepository.GetByIdAsync(historical.StudentId, ct);
            items.Add(new ActivityGradeItemDto(
                historical.StudentId,
                student?.FullName ?? "Aluno não encontrado",
                historical.Id,
                historical.GradeValue,
                true,
                true));
        }

        return Result.Success(new ActivityGradesDto(request.ActivityId, activity.SchoolClassId.Value, items));
    }

    private async Task<Result> AuthorizeReadAsync(Guid requestingUserId, Activity activity, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(requestingUserId, ct);
        if (user is null)
            return Result.Failure("Usuário não encontrado.");

        if (user.Role is UserRole.Coordenador or UserRole.Diretor)
            return Result.Success();

        if (user.Role == UserRole.Professor && activity.TeacherId == requestingUserId)
            return Result.Success();

        return Result.Failure("Usuário sem permissão para consultar notas desta atividade.");
    }
}

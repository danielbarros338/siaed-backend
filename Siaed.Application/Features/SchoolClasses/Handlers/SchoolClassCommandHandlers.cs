using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.SchoolClasses.Commands;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.SchoolClasses.Handlers;

public sealed class CreateSchoolClassCommandHandler : IRequestHandler<CreateSchoolClassCommand, Result<Guid>>
{
    private readonly ISchoolClassRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSchoolClassCommandHandler(
        ISchoolClassRepository repository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = SchoolClass.Create(request.Name, request.Grade, request.SchoolYear);

        if (request.TeacherIds is { Count: > 0 })
        {
            foreach (var teacherId in request.TeacherIds)
            {
                var user = await _userRepository.GetByIdAsync(teacherId, cancellationToken);
                if (user is null || user.Role != UserRole.Professor)
                    return Result<Guid>.Failure($"Professor '{teacherId}' não encontrado ou sem perfil de professor.");

                schoolClass.AssignTeacher(user);
            }
        }

        await _repository.AddAsync(schoolClass, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result<Guid>.Success(schoolClass.Id);
    }
}

public sealed class UpdateSchoolClassCommandHandler : IRequestHandler<UpdateSchoolClassCommand, Result>
{
    private readonly ISchoolClassRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSchoolClassCommandHandler(
        ISchoolClassRepository repository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _repository.GetByIdWithTeachersAsync(request.Id, cancellationToken);
        if (schoolClass is null)
            return Result.Failure("Turma não encontrada.");

        schoolClass.Update(request.Name, request.Grade, request.SchoolYear);

        if (request.TeacherIds is not null)
        {
            foreach (var id in schoolClass.Teachers.Select(t => t.Id).ToList())
                schoolClass.RemoveTeacher(id);

            foreach (var teacherId in request.TeacherIds)
            {
                var user = await _userRepository.GetByIdAsync(teacherId, cancellationToken);
                if (user is null || user.Role != UserRole.Professor)
                    return Result.Failure($"Professor '{teacherId}' não encontrado ou sem perfil de professor.");

                schoolClass.AssignTeacher(user);
            }
        }

        _repository.Update(schoolClass);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateSchoolClassCommandHandler : IRequestHandler<DeactivateSchoolClassCommand, Result>
{
    private readonly ISchoolClassRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSchoolClassCommandHandler(ISchoolClassRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (schoolClass is null)
            return Result.Failure("Turma não encontrada.");

        schoolClass.Deactivate();
        _repository.Update(schoolClass);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ReactivateSchoolClassCommandHandler : IRequestHandler<ReactivateSchoolClassCommand, Result>
{
    private readonly ISchoolClassRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateSchoolClassCommandHandler(ISchoolClassRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReactivateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (schoolClass is null)
            return Result.Failure("Turma não encontrada.");

        schoolClass.Reactivate();
        _repository.Update(schoolClass);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

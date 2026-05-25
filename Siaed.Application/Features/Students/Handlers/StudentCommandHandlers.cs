using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Students.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.Students.Handlers;

public sealed class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterStudentCommandHandler(
        IStudentRepository studentRepository,
        ISchoolClassRepository classRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (schoolClass is null)
            return Result<Guid>.Failure("Turma não encontrada.");

        var exists = await _studentRepository.ExistsByDocumentAsync(request.DocumentId, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Já existe um aluno com este documento.");

        var student = Student.Create(
            request.FullName,
            request.DocumentType,
            request.DocumentId,
            request.BirthDate,
            request.ClassId,
            request.EnrollmentDate,
            request.Notes);

        await _studentRepository.AddAsync(student, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(student.Id);
    }
}

public sealed class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result>
{
    private readonly IStudentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentCommandHandler(IStudentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null)
            return Result.Failure("Aluno não encontrado.");

        if (student.DocumentId != request.DocumentId)
        {
            var exists = await _repository.ExistsByDocumentAsync(request.DocumentId, cancellationToken);
            if (exists)
                return Result.Failure("Já existe um aluno com este documento.");
        }

        student.Update(request.FullName, request.DocumentType, request.DocumentId, request.BirthDate, request.Notes);
        _repository.Update(student);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class TransferStudentCommandHandler : IRequestHandler<TransferStudentCommand, Result>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferStudentCommandHandler(
        IStudentRepository studentRepository,
        ISchoolClassRepository classRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(TransferStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return Result.Failure("Aluno não encontrado.");

        var schoolClass = await _classRepository.GetByIdAsync(request.NewClassId, cancellationToken);
        if (schoolClass is null)
            return Result.Failure("Turma de destino não encontrada.");

        try
        {
            student.Transfer(request.NewClassId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _studentRepository.Update(student);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class DeactivateStudentCommandHandler : IRequestHandler<DeactivateStudentCommand, Result>
{
    private readonly IStudentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateStudentCommandHandler(IStudentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return Result.Failure("Aluno não encontrado.");

        try
        {
            student.Deactivate(request.NewStatus);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        _repository.Update(student);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class ReactivateStudentCommandHandler : IRequestHandler<ReactivateStudentCommand, Result>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateStudentCommandHandler(
        IStudentRepository studentRepository,
        ISchoolClassRepository classRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReactivateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return Result.Failure("Aluno não encontrado.");

        var schoolClass = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (schoolClass is null)
            return Result.Failure("Turma não encontrada.");

        student.Reactivate(request.ClassId);
        _studentRepository.Update(student);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}

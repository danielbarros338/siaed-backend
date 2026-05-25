using FluentValidation;
using Siaed.Application.Features.Students.Commands;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Students.Validators;

public sealed class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.BirthDate).LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.EnrollmentDate).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}

public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.BirthDate).LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}

public sealed class DeactivateStudentCommandValidator : AbstractValidator<DeactivateStudentCommand>
{
    public DeactivateStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.NewStatus).Must(s => s == StudentStatus.Inativo || s == StudentStatus.Evadido)
            .WithMessage("O status deve ser Inativo ou Evadido.");
    }
}

public sealed class ImportStudentsFromCsvCommandValidator : AbstractValidator<ImportStudentsFromCsvCommand>
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public ImportStudentsFromCsvCommandValidator()
    {
        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("O arquivo CSV é obrigatório.")
            .Must(c => c.Length <= MaxFileSizeBytes).WithMessage("O arquivo não pode exceder 5MB.");
    }
}

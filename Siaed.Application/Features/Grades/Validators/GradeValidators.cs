using FluentValidation;
using Siaed.Application.Features.Grades.Commands;
using Siaed.Application.Features.Grades.Queries;

namespace Siaed.Application.Features.Grades.Validators;

public sealed class CreateGradeCommandValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.SchoolClassId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.GradeValue).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ConventionKey).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
{
    public UpdateGradeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.GradeValue).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ConventionKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class ListGradesQueryValidator : AbstractValidator<ListGradesQuery>
{
    public ListGradesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.GradeValue).MaximumLength(30).When(x => x.GradeValue is not null);
    }
}

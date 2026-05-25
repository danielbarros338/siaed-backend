using FluentValidation;
using Siaed.Application.Features.Activities.Commands;

namespace Siaed.Application.Features.Activities.Validators;

public sealed class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(50);
        RuleFor(x => x.GradeConventionKey).MaximumLength(100).When(x => x.GradeConventionKey is not null);
        RuleFor(x => x.AgeRange).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Content).NotEmpty();
    }
}

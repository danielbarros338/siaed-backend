using FluentValidation;
using Siaed.Application.Features.Activities.Commands;

namespace Siaed.Application.Features.Activities.Validators;

public sealed class GenerateActivityCommandValidator : AbstractValidator<GenerateActivityCommand>
{
    public GenerateActivityCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AgeRange).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NumberOfQuestions).InclusiveBetween(1, 100);
    }
}

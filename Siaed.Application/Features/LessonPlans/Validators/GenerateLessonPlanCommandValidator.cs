using FluentValidation;
using Siaed.Application.Features.LessonPlans.Commands;

namespace Siaed.Application.Features.LessonPlans.Validators;

public sealed class GenerateLessonPlanCommandValidator : AbstractValidator<GenerateLessonPlanCommand>
{
    public GenerateLessonPlanCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AgeRange).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).LessThanOrEqualTo(480);
        RuleFor(x => x.AdditionalInstructions).MaximumLength(500);
    }
}

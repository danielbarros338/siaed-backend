using FluentValidation;
using Siaed.Application.Features.LessonPlans.Commands;

namespace Siaed.Application.Features.LessonPlans.Validators;

public sealed class CreateLessonPlanCommandValidator : AbstractValidator<CreateLessonPlanCommand>
{
    public CreateLessonPlanCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).LessThanOrEqualTo(480);
        RuleFor(x => x.Objectives).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.AgeRange).NotEmpty().MaximumLength(50);
    }
}

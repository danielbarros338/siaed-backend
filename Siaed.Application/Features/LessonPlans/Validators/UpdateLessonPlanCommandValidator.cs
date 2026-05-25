using FluentValidation;
using Siaed.Application.Features.LessonPlans.Commands;

namespace Siaed.Application.Features.LessonPlans.Validators;

public sealed class UpdateLessonPlanCommandValidator : AbstractValidator<UpdateLessonPlanCommand>
{
    public UpdateLessonPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Objectives).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Methodology).NotEmpty();
        RuleFor(x => x.Resources).NotEmpty();
        RuleFor(x => x.Evaluation).NotEmpty();
    }
}

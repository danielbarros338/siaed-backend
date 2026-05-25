using FluentValidation;
using Siaed.Application.Features.Reports.Commands;

namespace Siaed.Application.Features.Reports.Validators;

public sealed class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.HistoricalReportCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(20);
    }
}

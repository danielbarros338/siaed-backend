using FluentValidation;
using Siaed.Application.Features.Reports.Commands;

namespace Siaed.Application.Features.Reports.Validators;

public sealed class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).NotNull();
        RuleFor(x => x.ParentCommunication).NotNull();
    }
}

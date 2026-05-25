using FluentValidation;
using Siaed.Application.Features.Reports.Commands;

namespace Siaed.Application.Features.Reports.Validators;

public sealed class UpdateReportCommandValidator : AbstractValidator<UpdateReportCommand>
{
    public UpdateReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Summary).NotNull();
        RuleFor(x => x.ParentCommunication).NotNull();
    }
}

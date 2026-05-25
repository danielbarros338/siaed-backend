using FluentValidation;
using Siaed.Application.Features.SchoolClasses.Commands;

namespace Siaed.Application.Features.SchoolClasses.Validators;

public sealed class CreateSchoolClassCommandValidator : AbstractValidator<CreateSchoolClassCommand>
{
    public CreateSchoolClassCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SchoolYear).InclusiveBetween(2000, 2100);
    }
}

public sealed class UpdateSchoolClassCommandValidator : AbstractValidator<UpdateSchoolClassCommand>
{
    public UpdateSchoolClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Grade).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SchoolYear).InclusiveBetween(2000, 2100);
    }
}

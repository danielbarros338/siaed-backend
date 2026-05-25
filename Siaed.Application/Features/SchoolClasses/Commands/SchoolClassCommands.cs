using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.SchoolClasses.Commands;

public sealed record CreateSchoolClassCommand(
    string Name,
    string Grade,
    int SchoolYear,
    List<Guid>? TeacherIds = null
) : IRequest<Result<Guid>>;

public sealed record UpdateSchoolClassCommand(
    Guid Id,
    string Name,
    string Grade,
    int SchoolYear,
    List<Guid>? TeacherIds = null
) : IRequest<Result>;

public sealed record DeactivateSchoolClassCommand(
    Guid Id
) : IRequest<Result>;

public sealed record ReactivateSchoolClassCommand(
    Guid Id
) : IRequest<Result>;

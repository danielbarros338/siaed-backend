using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record UpdateActivityCommand(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Description,
    string Content) : IRequest<Result>;

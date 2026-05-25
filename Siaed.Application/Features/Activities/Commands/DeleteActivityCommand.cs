using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record DeleteActivityCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

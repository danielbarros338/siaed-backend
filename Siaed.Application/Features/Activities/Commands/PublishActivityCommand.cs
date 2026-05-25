using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record PublishActivityCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

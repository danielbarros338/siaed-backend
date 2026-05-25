using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Activities.Commands;

public sealed record ArchiveActivityCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

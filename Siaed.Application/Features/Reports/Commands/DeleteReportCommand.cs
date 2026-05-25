using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record DeleteReportCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

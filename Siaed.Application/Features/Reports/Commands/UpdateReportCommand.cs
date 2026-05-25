using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record UpdateReportCommand(
    Guid Id,
    Guid RequestingUserId,
    Guid StudentId,
    string Content,
    string Summary,
    string ParentCommunication,
    bool IsAIGenerated) : IRequest<Result>;

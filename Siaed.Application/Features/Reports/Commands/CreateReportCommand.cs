using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record CreateReportCommand(
    Guid UserId,
    Guid StudentId,
    string Content,
    string Summary = "",
    string ParentCommunication = "",
    bool IsAIGenerated = false) : IRequest<Result<Guid>>;

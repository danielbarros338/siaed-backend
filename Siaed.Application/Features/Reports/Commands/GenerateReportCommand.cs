using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record GenerateReportCommand(
    Guid UserId,
    Guid StudentId,
    int HistoricalReportCount = 5,
    string AdditionalInstructions = "") : IRequest<Result<Guid>>;

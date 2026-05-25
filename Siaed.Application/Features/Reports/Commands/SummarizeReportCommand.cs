using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record SummarizeReportCommand(Guid Id, Guid RequestingUserId) : IRequest<Result<SummarizeReportResponseDto>>;

public sealed record SummarizeReportResponseDto(Guid ReportId, string Summary, int TokensUsed, decimal EstimatedCost);

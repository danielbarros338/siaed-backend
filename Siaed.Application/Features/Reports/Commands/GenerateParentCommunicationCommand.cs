using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.Reports.Commands;

public sealed record GenerateParentCommunicationCommand(Guid Id, Guid RequestingUserId) : IRequest<Result<ParentCommunicationResponseDto>>;

public sealed record ParentCommunicationResponseDto(Guid ReportId, string ParentCommunication, int TokensUsed, decimal EstimatedCost);

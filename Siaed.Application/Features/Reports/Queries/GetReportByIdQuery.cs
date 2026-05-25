using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Reports.DTOs;

namespace Siaed.Application.Features.Reports.Queries;

public sealed record GetReportByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<PedagogicalReportDto>>;

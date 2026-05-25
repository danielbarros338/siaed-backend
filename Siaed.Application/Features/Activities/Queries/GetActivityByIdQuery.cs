using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Activities.DTOs;

namespace Siaed.Application.Features.Activities.Queries;

public sealed record GetActivityByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<Result<ActivityDto>>;

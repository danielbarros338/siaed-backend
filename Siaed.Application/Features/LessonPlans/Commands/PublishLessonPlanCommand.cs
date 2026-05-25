using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record PublishLessonPlanCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

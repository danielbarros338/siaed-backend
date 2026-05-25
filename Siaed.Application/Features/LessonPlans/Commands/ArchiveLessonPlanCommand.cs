using MediatR;
using Siaed.Application.Common;

namespace Siaed.Application.Features.LessonPlans.Commands;

public sealed record ArchiveLessonPlanCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.DTOs;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Auth.Commands;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role
) : IRequest<Result<RegisterResponseDto>>;

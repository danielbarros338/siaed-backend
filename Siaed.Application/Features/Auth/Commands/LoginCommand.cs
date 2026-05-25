using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.DTOs;

namespace Siaed.Application.Features.Auth.Commands;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponseDto>>;

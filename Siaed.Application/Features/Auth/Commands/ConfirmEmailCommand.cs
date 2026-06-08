using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.DTOs;

namespace Siaed.Application.Features.Auth.Commands
{
    public sealed record ConfirmEmailCommand(
        string Token
    ) : IRequest<Result<AuthResponseDto>>;
}

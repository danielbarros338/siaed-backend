using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Auth.DTOs;

public sealed record AuthResponseDto(
    Guid UserId,
    string Name,
    string Email,
    UserRole Role,
    string Token,
    DateTime ExpiresAt
);

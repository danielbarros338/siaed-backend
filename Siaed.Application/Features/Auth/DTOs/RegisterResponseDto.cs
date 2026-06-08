namespace Siaed.Application.Features.Auth.DTOs
{
    public sealed record RegisterResponseDto(
        string email,
        string name,
        string message
    );
}

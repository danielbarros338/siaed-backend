using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.Commands;
using Siaed.Application.Features.Auth.DTOs;
using Siaed.Application.Interfaces;

namespace Siaed.Application.Features.Auth.Handlers;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null)
            return Result<AuthResponseDto>.Failure("Credenciais inválidas.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("Usuário inativo. Entre em contato com o administrador.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Credenciais inválidas.");

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            token,
            expiresAt
        ));
    }
}

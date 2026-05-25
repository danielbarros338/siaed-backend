using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.Commands;
using Siaed.Application.Features.Auth.DTOs;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.Auth.Handlers;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await _userRepository.ExistsByEmailAsync(request.Email, ct);
        if (exists)
            return Result<AuthResponseDto>.Failure("E-mail já cadastrado.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash, request.Role);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

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

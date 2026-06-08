using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.Commands;
using Siaed.Application.Features.Auth.DTOs;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Application.Interfaces.Services;
using Siaed.Domain.Entities;

namespace Siaed.Application.Features.Auth.Handlers;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await _userRepository.ExistsByEmailAsync(request.Email, ct);
        if (exists)
            return Result<RegisterResponseDto>.Failure("E-mail já cadastrado.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, passwordHash, request.Role);

        var activationToken = Guid.NewGuid().ToString();
        user.SetActivationToken(activationToken);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        var base64Token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(activationToken)
        );

        await _emailService.SendConfirmEmail(user.Name, user.Email, base64Token);

        return Result<RegisterResponseDto>.Success(new RegisterResponseDto(
            user.Email,
            user.Name,
            "Registro realizado com sucesso. Enviamos um e-mail de confirmação para o seu endereço de e-mail."
        ));
    }
}

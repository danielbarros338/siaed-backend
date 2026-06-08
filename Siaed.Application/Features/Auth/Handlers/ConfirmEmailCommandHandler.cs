using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Auth.Commands;
using Siaed.Application.Features.Auth.DTOs;
using Siaed.Application.Interfaces;
using Siaed.Application.Interfaces.Repositories;

namespace Siaed.Application.Features.Auth.Handlers
{
    public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public readonly IJwtService _jwtService;

        public ConfirmEmailCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<Result<AuthResponseDto>> Handle(
            ConfirmEmailCommand request,
            CancellationToken ct
        )
        {
            var desserializedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));

            var user = await _userRepository.GetByActivationTokenAsync(desserializedToken, ct);
            if (user is null)
                return Result<AuthResponseDto>.Failure("Credenciais inválidas.");

            if (user.IsActive)
                return Result<AuthResponseDto>.Failure("Conta já ativada.");

            user.Activate();

            await _userRepository.UpdateAsync(user, ct);
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
}

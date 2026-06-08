using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Teachers.DTOs;
using Siaed.Application.Features.Teachers.Queries;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Teachers.Handlers;

public sealed class GetMyTeacherProfileQueryHandler
    : IRequestHandler<GetMyTeacherProfileQuery, Result<TeacherDto>>
{
    private readonly IUserRepository _userRepository;

    public GetMyTeacherProfileQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<Result<TeacherDto>> Handle(GetMyTeacherProfileQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);

        if (user is null || user.Role != UserRole.Professor)
            return Result.Failure<TeacherDto>("Perfil de professor não encontrado.");

        return Result.Success(new TeacherDto(user.Id, user.Name, user.Email, user.CreatedAt));
    }
}

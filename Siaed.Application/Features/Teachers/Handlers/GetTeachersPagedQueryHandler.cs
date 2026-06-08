using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Teachers.DTOs;
using Siaed.Application.Features.Teachers.Queries;
using Siaed.Application.Interfaces.Repositories;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Teachers.Handlers;

public sealed class GetTeachersPagedQueryHandler
    : IRequestHandler<GetTeachersPagedQuery, Result<PagedResult<TeacherDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetTeachersPagedQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<Result<PagedResult<TeacherDto>>> Handle(
        GetTeachersPagedQuery request,
        CancellationToken cancellationToken)
    {
        var paged = await _userRepository.ListByRoleAsync(
            UserRole.Professor, request.Page, request.PageSize, request.Search, cancellationToken);

        var dto = new PagedResult<TeacherDto>
        {
            Items = paged.Items.Select(u => new TeacherDto(u.Id, u.Name, u.Email, u.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Result<PagedResult<TeacherDto>>.Success(dto);
    }
}

namespace Siaed.Application.Features.Teachers.DTOs;

public sealed record TeacherDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt);

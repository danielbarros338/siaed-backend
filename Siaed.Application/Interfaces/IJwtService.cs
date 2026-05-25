using Siaed.Domain.Entities;

namespace Siaed.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

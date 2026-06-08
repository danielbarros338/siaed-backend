namespace Siaed.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendConfirmEmail(string name, string email, string activationToken);
    }
}

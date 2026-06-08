
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Siaed.Application.Interfaces.Services;

namespace Siaed.Infra.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public async Task SendConfirmEmail(string name, string email, string activationToken)
        {
            var link = $"{_configuration["UrlApi"]}/auth/activate?token={activationToken}";
            var htmlBody = Templates.ConfirmEmailTemplate.GetTemplate(name, link);

            await SendAsync(email, "Confirmação de Email", htmlBody);
        }

        private async Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var message = new MimeMessage();
                var mailboxAddress = new MailboxAddress(
                    _configuration["Email:EmailSettings:DisplayName"],
                    _configuration["Email:EmailSettings:From"]
                );

                message.From.Add(mailboxAddress);
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _configuration["Email:EmailSettings:Host"],
                    int.Parse(_configuration["Email:EmailSettings:Port"]),
                    SecureSocketOptions.StartTls,
                    cancellationToken
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:EmailSettings:Username"],
                    _configuration["Email:EmailSettings:Password"],
                    cancellationToken
                );

                await smtp.SendAsync(message, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                throw new Exception("Failed to send email", ex); // TODO: Criar middleware de excessões
            }
        }
    }
}

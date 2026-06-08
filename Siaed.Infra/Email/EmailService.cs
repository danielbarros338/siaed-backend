
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
                var displayName = _configuration["Email:EmailSettings:DisplayName"];
                var from = _configuration["Email:EmailSettings:From"];
                var host = _configuration["Email:EmailSettings:Host"];
                var portRaw = _configuration["Email:EmailSettings:Port"];
                var username = _configuration["Email:EmailSettings:Username"];
                var password = _configuration["Email:EmailSettings:Password"];

                if (string.IsNullOrWhiteSpace(from) ||
                    string.IsNullOrWhiteSpace(host) ||
                    string.IsNullOrWhiteSpace(portRaw) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException("Configuração de e-mail incompleta. Verifique Email:EmailSettings (Host, Port, Username, Password e From).");
                }

                if (!int.TryParse(portRaw, out var port))
                {
                    throw new InvalidOperationException("Configuração de e-mail inválida. Email:EmailSettings:Port deve ser numérico.");
                }

                var message = new MimeMessage();
                var mailboxAddress = new MailboxAddress(
                    displayName,
                    from
                );

                message.From.Add(mailboxAddress);
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    host,
                    port,
                    SecureSocketOptions.StartTls,
                    cancellationToken
                );

                await smtp.AuthenticateAsync(
                    username,
                    password,
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

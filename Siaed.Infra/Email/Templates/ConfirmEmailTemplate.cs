namespace Siaed.Infra.Email.Templates
{
    public static class ConfirmEmailTemplate
    {
        public static string GetTemplate(string name, string link)
        {
            return $@"
                <html>
                    <body>
                        <p>Olá {name},</p>
                        <p>Obrigado por se registrar no Siaed! Clique no link abaixo para confirmar seu email:</p>
                        <p><a href='{link}'>Confirmar Email</a></p>
                        <p>Se você não criou uma conta, por favor ignore este email.</p>
                        <p>Atenciosamente,<br/>Equipe Siaed</p>
                    </body>
                </html>";
        }
    }
}

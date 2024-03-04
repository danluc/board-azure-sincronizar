using Back.Dominio.Helpers;
using Back.Dominio.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Back.Servico.Email
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private bool MyCertHandler(object sender, X509Certificate certificado, X509Chain cadeia, SslPolicyErrors erro) => true;

        private EmailModel InstanciaNovoEmail(IEnumerable<string> destinatarios, string corpoEmail, string remetente, string usuario, string senha, int porta, string smtp)
        {
            var email = new EmailModel
            {
                Smtp = smtp,
                ConexaoSegura = true,
                Autenticacao = true,
                Usuario = usuario,
                SenhaMail = senha,
                PortaSmtp = porta.ToString(),
                Remetente = remetente,
                Destinatarios = destinatarios,
                Assunto = $"Sincronizar Board - {DateTime.Now.ToString("G")}",
                Corpo = corpoEmail
            };

            return email;
        }

        private void EnviarEmail(EmailModel emailModel)
        {
            ServicePointManager.ServerCertificateValidationCallback = MyCertHandler;
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient(emailModel.Smtp);
            //para quem vai ser enviado o email
            //message.To.Add(emailModel.Remetente);
            message.Subject = emailModel.Assunto;
            message.From = new MailAddress(emailModel.Usuario, "Gerenciatur");
            foreach (var item in emailModel.Destinatarios)
            {
                message.To.Add(item);
            }

            message.Body = emailModel.Corpo;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;

            //configurações da conta de envio                
            smtp.Host = emailModel.Smtp;
            smtp.EnableSsl = emailModel.ConexaoSegura;
            smtp.Port = int.Parse(emailModel.PortaSmtp);
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(emailModel.Usuario, emailModel.SenhaMail);

            //envio do email
            smtp.SendMailAsync(message);
        }

        public void EmailSeincronizar(Configuracao dados, string corpo, string destinatario)
        {
            try
            {
                string corpoEmail = ArquivosHtmlHelper.EmailSincronizar;
                corpoEmail = corpoEmail.Replace("[CORPO]", corpo);
                var emails = new List<string>();
                emails.Add(destinatario);

                var instancia = InstanciaNovoEmail(emails, corpoEmail, dados.Email, dados.Email, dados.Senha, dados.Porta, dados.SMTP);

                EnviarEmail(instancia);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Erro enviar e-mail: {ex.Message}");
            }
        }
    }
}

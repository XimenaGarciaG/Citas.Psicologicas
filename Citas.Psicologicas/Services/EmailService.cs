using System.Net;
using System.Net.Mail;
using Citas.Psicologicas.Configuration;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _settings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings();
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string bodyHtml)
    {
        if (string.IsNullOrWhiteSpace(to))
            return false;

        try
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.Username))
            {
                _logger.LogInformation("[Simulación SMTP] Correo enviado a {To} | Asunto: {Subject}", to, subject);
                return true;
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Correo enviado exitosamente a {To} | Asunto: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {To}", to);
            return false;
        }
    }
}

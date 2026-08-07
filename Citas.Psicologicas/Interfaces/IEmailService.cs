namespace Citas.Psicologicas.Interfaces;

/// <summary>Servicio para envío de correos electrónicos vía SMTP</summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string bodyHtml);
}

namespace Citas.Psicologicas.Configuration;

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SenderEmail { get; set; } = "no-reply@citaspsicologicas.edu.mx";
    public string SenderName { get; set; } = "Sistema Citas Psicológicas";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

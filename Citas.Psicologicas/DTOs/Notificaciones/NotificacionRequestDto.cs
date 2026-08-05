namespace Citas.Psicologicas.DTOs.Notificaciones;

/// <summary>DTO para enviar un recordatorio/aviso de cita por correo vía POST /notificaciones/enviar</summary>
public class NotificacionRequestDto
{
    public string EmailDestino { get; set; } = string.Empty;
    public string NombrePaciente { get; set; } = string.Empty;
    public string FechaCita { get; set; } = string.Empty; // YYYY-MM-DD
    public string HoraCita { get; set; } = string.Empty;   // HH:mm
}

/// <summary>Respuesta de POST /notificaciones/enviar</summary>
public class NotificacionResponseDto
{
    public string? Mensaje { get; set; }
}
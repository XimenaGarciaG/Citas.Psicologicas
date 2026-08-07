using System.Threading.Tasks;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el envío de notificaciones por correo</summary>
public interface INotificacionService
{
    /// <summary>Envía un recordatorio/aviso de cita por correo mediante la API</summary>
    Task<bool> EnviarRecordatorioAsync(DTOs.Notificaciones.NotificacionRequestDto dto, string? token = null);

    /// <summary>
    /// Envía un correo con asunto y cuerpo HTML personalizados (restablecer
    /// contraseña, bitácora) mediante la API.
    /// </summary>
    Task<bool> EnviarCorreoPersonalizadoAsync(string emailDestino, string asunto, string cuerpoHtml);
}
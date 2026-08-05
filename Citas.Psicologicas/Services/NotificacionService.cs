using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Notificaciones;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para envío de notificaciones de citas por correo</summary>
public class NotificacionService : BaseApiService, INotificacionService
{
    public NotificacionService(IHttpClientFactory httpClientFactory, ILogger<NotificacionService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<bool> EnviarRecordatorioAsync(NotificacionRequestDto dto, string? token = null)
    {
        var result = await PostAsync<NotificacionResponseDto>(ApiRoutes.NotificacionesEnviar, dto, token);
        if (!result.Success)
        {
            Logger.LogWarning("POST notificaciones/enviar -> {Message}", result.Message);
            return false;
        }
        return true;
    }
}
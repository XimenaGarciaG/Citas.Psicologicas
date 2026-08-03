using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Interfaces;

/// <summary>
/// Contrato para la capa de datos locales (respaldo en JSON).
/// Se utiliza para las funciones que la API REST hosteada aún no expone:
/// catálogos, configuración, historial de notificaciones, seguimientos,
/// confirmaciones de asistencia, contraseñas locales y tokens de recuperación.
/// </summary>
public interface ILocalDataService
{
    List<Carrera> GetCarreras();
    void SaveCarreras(List<Carrera> carreras);

    List<Grupo> GetGrupos();
    void SaveGrupos(List<Grupo> grupos);

    ConfiguracionSistema GetConfiguracion();
    void SaveConfiguracion(ConfiguracionSistema config);

    List<NotificacionRegistro> GetNotificaciones();
    void AddNotificacion(NotificacionRegistro notificacion);

    List<SeguimientoRegistro> GetSeguimientos();
    void AddSeguimiento(SeguimientoRegistro seguimiento);
    void UpdateSeguimiento(SeguimientoRegistro seguimiento);

    List<ConfirmacionAsistencia> GetConfirmaciones();
    void SetConfirmacion(ConfirmacionAsistencia confirmacion);
    ConfirmacionAsistencia? GetConfirmacion(string idCita);

    void SetContrasenaLocal(string userId, string password);
    string? GetContrasenaLocal(string userId);
    void SetContrasenaLocalPorCorreo(string correo, string password);
    string? GetContrasenaLocalPorCorreo(string correo);
    void SetUsuarioActivoLocal(string userId, bool activo);
    bool? GetUsuarioActivoLocal(string userId);

    void SetResetToken(string email, string token);
    string? GetResetToken(string email);
    void RemoveResetToken(string email);
}

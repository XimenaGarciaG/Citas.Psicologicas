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
    void MarcarNotificacionEnviada(int id);

    List<SeguimientoRegistro> GetSeguimientos();
    void AddSeguimiento(SeguimientoRegistro seguimiento);
    void UpdateSeguimiento(SeguimientoRegistro seguimiento);

    List<BitacoraPendiente> GetBitacorasPendientes();
    BitacoraPendiente? GetBitacoraPendiente(string idCita);
    void AddBitacoraPendiente(BitacoraPendiente bitacora);
    void UpdateBitacoraPendiente(BitacoraPendiente bitacora);

    List<ConfirmacionAsistencia> GetConfirmaciones();
    void SetConfirmacion(ConfirmacionAsistencia confirmacion);
    ConfirmacionAsistencia? GetConfirmacion(string idCita);

    List<ReagendaRegistro> GetReagendas();
    ReagendaRegistro? GetReagenda(string idCita);
    void AddReagenda(ReagendaRegistro reagenda);

    List<BloqueoDisponibilidad> GetBloqueos();
    List<BloqueoDisponibilidad> GetBloqueos(DateTime fecha);
    void AddBloqueo(BloqueoDisponibilidad bloqueo);
    void RemoveBloqueo(int id);
    bool TieneBloqueo(string idPsicologo, DateTime fecha, string horaInicio, string horaFin);

    List<SolicitudCalendario> GetSolicitudesCalendario();
    List<SolicitudCalendario> GetSolicitudesCalendarioPendientes();
    void AddSolicitudCalendario(SolicitudCalendario solicitud);
    void MarcarSolicitudCalendarioAtendida(int id);

    List<CanalizacionSolicitud> GetCanalizacionesSolicitudes();
    void AddCanalizacionSolicitud(CanalizacionSolicitud vinculo);

    string? GetPsicologaEncargadaId();
    void SetPsicologaEncargadaId(string? idUsuario);

    void SetContrasenaLocal(string userId, string password);
    string? GetContrasenaLocal(string userId);
    void SetContrasenaLocalPorCorreo(string correo, string password);
    string? GetContrasenaLocalPorCorreo(string correo);
    void SetUsuarioActivoLocal(string userId, bool activo);
    bool? GetUsuarioActivoLocal(string userId);

    void SetResetToken(string email, string token);
    string? GetResetToken(string email);
    string? GetEmailByResetToken(string token);
    void RemoveResetToken(string email);
}

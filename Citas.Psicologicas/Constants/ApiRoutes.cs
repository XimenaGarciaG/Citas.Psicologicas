namespace Citas.Psicologicas.Constants;

/// <summary>Rutas de la API REST consumida por el sistema</summary>
public static class ApiRoutes
{
    // Auth
    public const string Login = "auth/login";
    public const string Logout = "auth/logout";

    // Usuarios
    public const string Usuarios = "api/Usuarios";
    public const string UsuarioById = "api/Usuarios/{0}";

    // Solicitudes
    public const string Solicitudes = "solicitudes";
    public const string SolicitudById = "solicitudes/{0}";
    public const string SolicitudPrioridad = "solicitudes/{0}/prioridad";

    // Canalizaciones
    public const string Canalizaciones = "canalizaciones";

    // Citas
    public const string Citas = "citas";
    public const string CitaById = "citas/{0}";
    public const string CitaConfirmar = "citas/{0}/confirmar";
    public const string CitaCancelar = "citas/{0}/cancelar";
    public const string CitaReagendar = "citas/{0}/reagendar";

    // Bitácora
    public const string Bitacora = "bitacora";

    // Notificaciones
    public const string NotificacionesEnviar = "notificaciones/enviar";

    // Reportes
    public const string ReportesAsistencia = "reportes/asistencia";
    public const string ReportesInasistencia = "reportes/inasistencia";

    // Dashboard
    public const string Dashboard = "dashboard";
}

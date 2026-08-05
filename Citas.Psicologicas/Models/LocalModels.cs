namespace Citas.Psicologicas.Models;

/// <summary>Catálogo de carreras institucionales (respaldo local)</summary>
public class Carrera
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Catálogo de grupos institucionales (respaldo local)</summary>
public class Grupo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Parámetros configurables del sistema (respaldo local)</summary>
public class ConfiguracionSistema
{
    public string HorarioInicio { get; set; } = "08:00";
    public string HorarioFin { get; set; } = "18:00";
    public int DuracionCitaMin { get; set; } = 60;
    public int MinutosTolerancia { get; set; } = 15;
    public int VentanaCancelacionHoras { get; set; } = 24;
    public int VentanaConfirmacionHoras { get; set; } = 24;
    public int RecordatorioHorasAntes { get; set; } = 24;
}

/// <summary>Registro de notificación/correo enviado (respaldo local)</summary>
public class NotificacionRegistro
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "RECORDATORIO"; // Confirmacion, Recordatorio, Reagenda, Cancelacion
    public string IdEstudiante { get; set; } = string.Empty;
    public string CorreoDestinatario { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public string EnviadoPor { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
}

/// <summary>Registro de seguimiento derivado de la bitácora (respaldo local)</summary>
public class SeguimientoRegistro
{
    public int Id { get; set; }
    public string IdCita { get; set; } = string.Empty;
    public string IdSolicitud { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string IdPsicologo { get; set; } = string.Empty;
    public string NombrePsicologo { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public string? Notas { get; set; }
    public bool Programado { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>Bitácora enviada por la psicóloga pendiente de confirmación del estudiante (respaldo local)</summary>
public class BitacoraPendiente
{
    public int Id { get; set; }
    public string IdCita { get; set; } = string.Empty;
    public string IdSolicitud { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public string IdPsicologo { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string NombrePsicologo { get; set; } = string.Empty;
    public bool Asistencia { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public bool AcuerdoSeguimiento { get; set; }
    public DateTime FechaEnvio { get; set; } = DateTime.Now;
    public bool Confirmada { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string Estado => Confirmada ? "CONFIRMADA" : "PENDIENTE";
}

/// <summary>Registro de confirmación electrónica de asistencia del estudiante (respaldo local)</summary>
public class ConfirmacionAsistencia
{
    public string IdCita { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public bool Confirmada { get; set; }
    public DateTime FechaConfirmacion { get; set; }
}

/// <summary>Registro de reagenda de una cita (un alumno solo puede reagendar una vez por cita)</summary>
public class ReagendaRegistro
{
    public int Id { get; set; }
    public string IdCita { get; set; } = string.Empty;
    public string IdSolicitud { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public DateTime FechaAnterior { get; set; }
    public string HoraInicioAnterior { get; set; } = string.Empty;
    public DateTime NuevaFecha { get; set; }
    public string NuevaHoraInicio { get; set; } = string.Empty;
    public string? MotivoReagenda { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>Bloqueo de disponibilidad de una psicóloga para un horario específico (respaldo local)</summary>
public class BloqueoDisponibilidad
{
    public int Id { get; set; }
    public string IdPsicologo { get; set; } = string.Empty;
    public string NombrePsicologo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>Solicitud generada desde el calendario de disponibilidad hacia una psicóloga concreta</summary>
public class SolicitudCalendario
{
    public int Id { get; set; }
    public string IdSolicitud { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string IdPsicologo { get; set; } = string.Empty;
    public string NombrePsicologo { get; set; } = string.Empty;
    public DateTime FechaCita { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public bool Atendida { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>Marca local del usuario designado como Psicóloga Encargada (respaldo local)</summary>
public class PsicologaEncargada
{
    public string IdUsuario { get; set; } = string.Empty;
}

/// <summary>Vínculo entre una canalización del tutor y la solicitud de atención generada por ella</summary>
public class CanalizacionSolicitud
{
    public int Id { get; set; }
    public string IdCanalizacion { get; set; } = string.Empty;
    public string IdSolicitud { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string IdTutor { get; set; } = string.Empty;
    public string NombreTutor { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

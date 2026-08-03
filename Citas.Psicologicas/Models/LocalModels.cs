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
    public string? Motivo { get; set; }
    public bool Programado { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

/// <summary>Confirmación electrónica de asistencia del estudiante (respaldo local)</summary>
public class ConfirmacionAsistencia
{
    public string IdCita { get; set; } = string.Empty;
    public string IdEstudiante { get; set; } = string.Empty;
    public bool Confirmada { get; set; }
    public DateTime FechaConfirmacion { get; set; }
}

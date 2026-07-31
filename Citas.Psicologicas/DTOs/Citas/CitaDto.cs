using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO de lectura para citas devuelto por GET /citas</summary>
public class CitaDto
{
    [JsonPropertyName("idCita")]
    public object? IdCita { get; set; }

    [JsonPropertyName("idSolicitud")]
    public object? IdSolicitud { get; set; }

    [JsonPropertyName("idEstudiante")]
    public object? IdEstudiante { get; set; }

    public string? NombreEstudiante { get; set; }

    [JsonPropertyName("idPsicologo")]
    public object? IdPsicologo { get; set; }

    public string? NombrePsicologo { get; set; }
    public DateTime? FechaCita { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public int MinutosTolerancia { get; set; } = 15;
    public string Estado { get; set; } = "RESERVADA";
    public string? Prioridad { get; set; }
    public string? Motivo { get; set; }
    public bool EsSeguimiento { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public string Id => IdCita?.ToString() ?? "";
    public string IdEstudianteStr => IdEstudiante?.ToString() ?? "";
    public string IdPsicologoStr => IdPsicologo?.ToString() ?? "";
    public DateTime Fecha => FechaCita ?? DateTime.Today;

    public string BadgeEstado => Estado?.ToUpper() switch
    {
        "RESERVADA" => "primary",
        "CONFIRMADA" => "success",
        "CANCELADA" => "danger",
        "CONCLUIDA" => "secondary",
        "REAGENDADA" => "warning",
        _ => "light"
    };

    public string ColorCalendario => Estado?.ToUpper() switch
    {
        "RESERVADA" => "#2563EB",
        "CONFIRMADA" => "#059669",
        "CANCELADA" => "#DC2626",
        "CONCLUIDA" => "#64748B",
        "REAGENDADA" => "#D97706",
        _ => "#94A3B8"
    };
}

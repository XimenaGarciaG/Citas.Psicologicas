using System.Text.Json;
using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO de lectura para citas devuelto por GET /citas</summary>
public class CitaDto
{
    [JsonPropertyName("idCita")]
    public JsonElement? IdCitaElement { get; set; }

    [JsonPropertyName("idSolicitud")]
    public JsonElement? IdSolicitudElement { get; set; }

    [JsonPropertyName("idEstudiante")]
    public JsonElement? IdEstudianteElement { get; set; }

    [JsonPropertyName("idPsicologo")]
    public JsonElement? IdPsicologoElement { get; set; }

    public string? NombreEstudiante { get; set; }
    public string? NombrePsicologo { get; set; }
    public DateTime? FechaCita { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public int MinutosTolerancia { get; set; } = 15;

    // FIX: Mapear explícitamente la propiedad que viene de la API REST ("estadoCita")
    [JsonPropertyName("estadoCita")]
    public string Estado { get; set; } = "RESERVADA";

    public string? Prioridad { get; set; }
    public string? Motivo { get; set; }
    public bool EsSeguimiento { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public string Id => IdCitaElement.HasValue ? IdCitaElement.Value.ToString() : "";
    public string IdEstudianteStr => IdEstudianteElement.HasValue ? IdEstudianteElement.Value.ToString() : "";
    public string IdPsicologoStr => IdPsicologoElement.HasValue ? IdPsicologoElement.Value.ToString() : "";
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
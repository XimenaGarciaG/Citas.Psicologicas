using Citas.Psicologicas.Constants;
using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Solicitudes;

/// <summary>DTO de lectura para solicitudes de atención devuelto por GET /solicitudes</summary>
public class SolicitudDto
{
    [JsonPropertyName("idSolicitud")]
    public object? IdSolicitud { get; set; }

    [JsonPropertyName("idEstudiante")]
    public object? IdEstudiante { get; set; }

    public string? NombreEstudiante { get; set; }
    public string? MatriculaEstudiante { get; set; }
    public string Origen { get; set; } = OrigenSolicitud.Autonoma;
    public string MotivoConsulta { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "BAJA";
    public int PuntuacionTriage { get; set; }
    public string EstadoSolicitud { get; set; } = "PENDIENTE";
    public DateTime? FechaSolicitud { get; set; }
    public string? IdPsicologo { get; set; }
    public string? NombrePsicologo { get; set; }

    public string Id => IdSolicitud?.ToString() ?? "";
    public string IdEstudianteStr => IdEstudiante?.ToString() ?? "";
    public string Estado => EstadoSolicitud;
    public string Comentario => MotivoConsulta;

    public string BadgeEstado => EstadoSolicitud?.ToUpper() switch
    {
        "PENDIENTE" => "warning",
        "AGENDADA" or "ATENDIDA" => "success",
        "CERRADA" or "CANCELADA" => "danger",
        _ => "secondary"
    };

    public string BadgePrioridad => Prioridad?.ToUpper() switch
    {
        "ALTA" => "danger",
        "MEDIA" => "warning",
        "BAJA" => "info",
        _ => "secondary"
    };
}

/// <summary>Respuesta al calcular la prioridad de una solicitud</summary>
public class UpdatePrioridadResponseDto
{
    public string? Mensaje { get; set; }
    public object? IdSolicitud { get; set; }
    public string? PrioridadCalculada { get; set; }
    public int PuntuacionTriage { get; set; }
}

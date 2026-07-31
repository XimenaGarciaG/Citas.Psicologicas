using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Canalizaciones;

/// <summary>DTO de lectura para canalizaciones devuelto por GET /canalizaciones</summary>
public class CanalizacionDto
{
    [JsonPropertyName("idCanalizacion")]
    public object? IdCanalizacion { get; set; }

    [JsonPropertyName("idTutor")]
    public object? IdTutor { get; set; }

    public string? NombreTutor { get; set; }

    [JsonPropertyName("idEstudiante")]
    public object? IdEstudiante { get; set; }

    public string? NombreEstudiante { get; set; }
    public string MotivoCanalizacion { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime? FechaCanalizacion { get; set; }

    public string Id => IdCanalizacion?.ToString() ?? "";
    public string Motivo => MotivoCanalizacion;
}

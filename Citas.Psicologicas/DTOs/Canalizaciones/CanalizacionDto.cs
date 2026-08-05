using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Canalizaciones;

public class CanalizacionDto
{
    [JsonPropertyName("idCanalizacion")]
    public object? IdCanalizacion { get; set; }

    [JsonPropertyName("idTutor")]
    public object? IdTutor { get; set; }

    [JsonPropertyName("idUsuarioTutor")]
    public object? IdUsuarioTutor { get; set; } // <-- Mapeo para verificar rol Tutor en FrontEnd

    [JsonPropertyName("nombreTutor")]
    public string? NombreTutor { get; set; }

    [JsonPropertyName("idEstudiante")]
    public object? IdEstudiante { get; set; }

    [JsonPropertyName("nombreEstudiante")]
    public string? NombreEstudiante { get; set; }

    [JsonPropertyName("motivoCanalizacion")]
    public string MotivoCanalizacion { get; set; } = string.Empty;

    [JsonPropertyName("observaciones")]
    public string Observaciones { get; set; } = string.Empty;

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "PENDIENTE";

    [JsonPropertyName("fechaCanalizacion")]
    public DateTime? FechaCanalizacion { get; set; }

    public string Id => IdCanalizacion?.ToString() ?? "";
    public string Motivo => MotivoCanalizacion;
}
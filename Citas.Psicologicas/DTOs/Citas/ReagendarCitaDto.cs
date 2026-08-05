using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para reagendar una cita existente en PATCH /citas/{id}/reagendar</summary>
public class ReagendarCitaDto
{
    [JsonPropertyName("idPsicologo")]
    public int IdPsicologo { get; set; }

    [JsonPropertyName("fechaCita")]
    public string FechaCita { get; set; } = string.Empty; // Formato YYYY-MM-DD para DateOnly

    [JsonPropertyName("horaInicio")]
    public string HoraInicio { get; set; } = string.Empty; // Formato HH:mm:ss para TimeOnly

    [JsonPropertyName("horaFin")]
    public string HoraFin { get; set; } = string.Empty; // Formato HH:mm:ss para TimeOnly

    [JsonPropertyName("minutosTolerancia")]
    public int MinutosTolerancia { get; set; } = 15;

    [JsonPropertyName("motivoReagenda")]
    public string? MotivoReagenda { get; set; }
}
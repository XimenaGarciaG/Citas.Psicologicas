namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para actualizar datos de una cita</summary>
public class UpdateCitaDto
{
    public DateTime Fecha { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para reagendar una cita existente</summary>
public class ReagendarCitaDto
{
    public DateTime NuevaFecha { get; set; }
    public string NuevaHoraInicio { get; set; } = string.Empty;
    public string NuevaHoraFin { get; set; } = string.Empty;
    public string? MotivoReagenda { get; set; }
}

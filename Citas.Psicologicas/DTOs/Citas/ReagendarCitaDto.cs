namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para reagendar una cita existente</summary>
public class ReagendarCitaDto
{
    public int IdPsicologo { get; set; }
    public string FechaCita { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public int MinutosTolerancia { get; set; } = 15;
    public string? MotivoReagenda { get; set; }
}

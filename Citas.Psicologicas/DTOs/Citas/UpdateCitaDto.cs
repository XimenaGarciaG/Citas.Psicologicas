namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para actualizar datos de una cita</summary>
public class UpdateCitaDto
{
    public int IdPsicologo { get; set; }
    public string FechaCita { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public int MinutosTolerancia { get; set; } = 15;
}

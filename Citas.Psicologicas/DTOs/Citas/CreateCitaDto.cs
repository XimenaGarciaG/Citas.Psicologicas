namespace Citas.Psicologicas.DTOs.Citas;

/// <summary>DTO para agendar una cita en POST /citas</summary>
public class CreateCitaDto
{
    public int IdSolicitud { get; set; } = 0;
    public int IdPsicologo { get; set; } = 0;
    public string FechaCita { get; set; } = string.Empty; // YYYY-MM-DD
    public string HoraInicio { get; set; } = string.Empty; // HH:mm:ss
    public string HoraFin { get; set; } = string.Empty; // HH:mm:ss
    public int MinutosTolerancia { get; set; } = 15;
}

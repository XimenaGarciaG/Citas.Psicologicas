namespace Citas.Psicologicas.DTOs.Solicitudes;

/// <summary>DTO para asignar prioridad a una solicitud (solo psicólogo)</summary>
public class UpdatePrioridadDto
{
    /// <summary>Alta, Media o Baja — Asignado exclusivamente por la psicóloga</summary>
    public string Prioridad { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Estado { get; set; }
}

namespace Citas.Psicologicas.DTOs.Canalizaciones;

/// <summary>DTO para crear una canalización en POST /canalizaciones</summary>
public class CreateCanalizacionDto
{
    public object IdTutor { get; set; } = 0;
    public object IdEstudiante { get; set; } = 0;
    public string MotivoCanalizacion { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
}

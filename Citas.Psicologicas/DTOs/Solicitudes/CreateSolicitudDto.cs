namespace Citas.Psicologicas.DTOs.Solicitudes;

/// <summary>DTO para crear una solicitud en POST /solicitudes</summary>
public class CreateSolicitudDto
{
    public object IdEstudiante { get; set; } = 0;
    public string Origen { get; set; } = "ESTUDIANTE";
    public string MotivoConsulta { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "BAJA";
    public int PuntuacionTriage { get; set; } = 0;
}

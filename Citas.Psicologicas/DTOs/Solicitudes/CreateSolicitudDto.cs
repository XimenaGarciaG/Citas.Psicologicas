using Citas.Psicologicas.Constants;

namespace Citas.Psicologicas.DTOs.Solicitudes;

/// <summary>DTO para crear una solicitud en POST /solicitudes</summary>
public class CreateSolicitudDto
{
    public int IdEstudiante { get; set; } = 0;
    public string Origen { get; set; } = OrigenSolicitud.Autonoma;
    public string MotivoConsulta { get; set; } = string.Empty;
    public string Prioridad { get; set; } = Prioridades.Baja;
    public int PuntuacionTriage { get; set; } = 0;
}

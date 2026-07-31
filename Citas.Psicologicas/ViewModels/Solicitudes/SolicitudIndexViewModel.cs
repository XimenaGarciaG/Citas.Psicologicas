using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Solicitudes;

namespace Citas.Psicologicas.ViewModels.Solicitudes;

/// <summary>ViewModel para el listado de solicitudes</summary>
public class SolicitudIndexViewModel
{
    public List<SolicitudDto> Solicitudes { get; set; } = [];
    public string? FiltroEstado { get; set; }
    public string? FiltroPrioridad { get; set; }
    public string? FiltroBusqueda { get; set; }
    public int Total => Solicitudes.Count;
    public int Pendientes => Solicitudes.Count(s => s.Estado == EstadosSolicitud.Pendiente);
}

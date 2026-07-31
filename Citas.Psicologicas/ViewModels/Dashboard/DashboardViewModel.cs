using Citas.Psicologicas.DTOs.Dashboard;

namespace Citas.Psicologicas.ViewModels.Dashboard;

/// <summary>ViewModel para el dashboard principal del sistema</summary>
public class DashboardViewModel
{
    public DashboardDto Estadisticas { get; set; } = new();
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaHoy { get; set; } = DateTime.Now;
}

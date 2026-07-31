using Citas.Psicologicas.DTOs.Canalizaciones;

namespace Citas.Psicologicas.ViewModels.Canalizaciones;

/// <summary>ViewModel para el listado de canalizaciones</summary>
public class CanalizacionIndexViewModel
{
    public List<CanalizacionDto> Canalizaciones { get; set; } = [];
    public string? FiltroBusqueda { get; set; }
    public string? FiltroEstado { get; set; }
}

using Citas.Psicologicas.DTOs.Canalizaciones;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.ViewModels.Canalizaciones;

/// <summary>ViewModel para el listado de canalizaciones</summary>
public class CanalizacionIndexViewModel
{
    public List<CanalizacionDto> Canalizaciones { get; set; } = [];
    public List<CanalizacionSolicitud> Vinculos { get; set; } = [];
    public string? FiltroBusqueda { get; set; }
    public string? FiltroEstado { get; set; }

    /// <summary>Solicitud generada por una canalización (si existe)</summary>
    public CanalizacionSolicitud? VinculoDe(string idCanalizacion)
        => Vinculos.FirstOrDefault(v => string.Equals(v.IdCanalizacion, idCanalizacion, StringComparison.OrdinalIgnoreCase));
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;

namespace Citas.Psicologicas.ViewModels.Citas;

/// <summary>ViewModel para el listado y calendario de citas</summary>
public class CitaIndexViewModel
{
    public List<CitaDto> Citas { get; set; } = [];
    public string? FiltroEstado { get; set; }
    public DateTime? FiltroFechaInicio { get; set; }
    public DateTime? FiltroFechaFin { get; set; }
    public string? FiltroBusqueda { get; set; }
    public string Vista { get; set; } = "Lista"; // Lista, Calendario
    public int Total => Citas.Count;
    public int Reservadas => Citas.Count(c => c.Estado == EstadosCita.Reservada);
    public int Confirmadas => Citas.Count(c => c.Estado == EstadosCita.Confirmada);
    public int Canceladas => Citas.Count(c => c.Estado == EstadosCita.Cancelada);
}

using Citas.Psicologicas.DTOs.Bitacora;

namespace Citas.Psicologicas.ViewModels.Bitacora;

/// <summary>ViewModel para el listado de registros de bitácora</summary>
public class BitacoraIndexViewModel
{
    public List<BitacoraDto> Registros { get; set; } = [];
    public string? FiltroBusqueda { get; set; }
    public DateTime? FiltroFecha { get; set; }
    public int TotalAsistencias => Registros.Count(r => r.Asistio);
    public int TotalInasistencias => Registros.Count(r => !r.Asistio);
}

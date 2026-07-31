using System.ComponentModel.DataAnnotations;
using Citas.Psicologicas.DTOs.Reportes;

namespace Citas.Psicologicas.ViewModels.Reportes;

/// <summary>ViewModel para filtros y resultados de reportes</summary>
public class ReporteFiltroViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha Inicio")]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha Fin")]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    [Display(Name = "Tipo de Reporte")]
    public string TipoReporte { get; set; } = "Asistencia";

    public List<ReporteAsistenciaDto>? DatosAsistencia { get; set; }
    public List<ReporteInasistenciaDto>? DatosInasistencia { get; set; }
    public bool TieneResultados => (DatosAsistencia?.Count > 0) || (DatosInasistencia?.Count > 0);
}

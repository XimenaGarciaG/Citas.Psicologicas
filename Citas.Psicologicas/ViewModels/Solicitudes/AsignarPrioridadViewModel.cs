using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Solicitudes;

/// <summary>ViewModel para que la psicóloga asigne prioridad a una solicitud</summary>
public class AsignarPrioridadViewModel
{
    public string IdSolicitud { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar una prioridad")]
    [Display(Name = "Prioridad")]
    public string Prioridad { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Estado")]
    public string? Estado { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Citas.Psicologicas.DTOs.Citas;

namespace Citas.Psicologicas.ViewModels.Bitacora;

/// <summary>ViewModel para registrar asistencia en bitácora</summary>
public class BitacoraCreateViewModel
{
    [Required(ErrorMessage = "Debe seleccionar una cita")]
    [Display(Name = "Cita")]
    public string IdCita { get; set; } = string.Empty;

    [Display(Name = "¿Asistió el estudiante?")]
    public bool Asistencia { get; set; }

    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    [Display(Name = "Observaciones")]
    [DataType(DataType.MultilineText)]
    public string? Observaciones { get; set; }

    [Display(Name = "¿Requiere cita de seguimiento?")]
    public bool AcuerdoSeguimiento { get; set; }

    public List<CitaDto> CitasSinRegistro { get; set; } = [];
}

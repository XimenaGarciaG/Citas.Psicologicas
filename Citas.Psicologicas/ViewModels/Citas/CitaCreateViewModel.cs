using System.ComponentModel.DataAnnotations;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.DTOs.Usuarios;

namespace Citas.Psicologicas.ViewModels.Citas;

/// <summary>ViewModel para agendar una nueva cita</summary>
public class CitaCreateViewModel
{
    [Required(ErrorMessage = "Debe seleccionar la solicitud de atención")]
    [Display(Name = "Solicitud de atención")]
    public string IdSolicitud { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un psicólogo")]
    [Display(Name = "Psicólogo/a")]
    public string IdPsicologo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de la cita")]
    public DateTime FechaCita { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "La hora de inicio es obligatoria")]
    [Display(Name = "Hora de inicio")]
    public string HoraInicio { get; set; } = "09:00";

    [Required(ErrorMessage = "La hora de fin es obligatoria")]
    [Display(Name = "Hora de fin")]
    public string HoraFin { get; set; } = "10:00";

    [Display(Name = "Minutos de tolerancia")]
    [Range(0, 120, ErrorMessage = "La tolerancia debe estar entre 0 y 120 minutos")]
    public int MinutosTolerancia { get; set; } = 15;

    public List<SolicitudDto> SolicitudesPendientes { get; set; } = [];
    public List<UsuarioDto> Psicologos { get; set; } = [];
}

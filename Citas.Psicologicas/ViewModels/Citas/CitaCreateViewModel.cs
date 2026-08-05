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

    /// <summary>Solicitud seleccionada en el GET (para detectar psicóloga específica)</summary>
    public SolicitudDto? SolicitudSeleccionada { get; set; }

    /// <summary>Psicóloga solicitada por el estudiante (si la solicitud fue específica)</summary>
    public string PsicologaSolicitadaId { get; set; } = string.Empty;
    public string PsicologaSolicitadaNombre { get; set; } = string.Empty;

    /// <summary>Indica si la solicitud fue hecha a una psicóloga concreta (solo ella agenda)</summary>
    public bool EsSolicitudEspecifica => !string.IsNullOrEmpty(PsicologaSolicitadaId);

    /// <summary>Mapa de psicólogas en horarios disponibles para la fecha (para la vista)</summary>
    public Dictionary<string, List<string>> DisponibilidadPorPsicologa { get; set; } = new();

    /// <summary>Horarios disponibles en la fecha (gris de referencia)</summary>
    public List<string> HorariosDisponibles { get; set; } = [];
}
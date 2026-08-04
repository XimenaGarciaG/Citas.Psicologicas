using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Solicitudes;

/// <summary>ViewModel para que el estudiante solicite atención psicológica</summary>
public class SolicitudCreateViewModel
{
    public string IdEstudiante { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres")]
    [Display(Name = "Comentario (Opcional)")]
    [DataType(DataType.MultilineText)]
    public string? Comentario { get; set; }

    // Datos de la solicitud directa desde el calendario de disponibilidad
    public string IdPsicologo { get; set; } = string.Empty;
    public string NombrePsicologo { get; set; } = string.Empty;
    public DateTime? FechaCita { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;

    public bool DesdeCalendario => !string.IsNullOrEmpty(IdPsicologo);
}

using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Solicitudes;

/// <summary>ViewModel para que el estudiante solicite atención psicológica</summary>
public class SolicitudCreateViewModel
{
    // Hacer anulables (?) los campos para que la validación de jQuery no los bloquee
    public string? IdEstudiante { get; set; }

    [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres")]
    [Display(Name = "Comentario (Opcional)")]
    [DataType(DataType.MultilineText)]
    public string? Comentario { get; set; }

    // Datos de la solicitud directa desde el calendario de disponibilidad
    public string? IdPsicologo { get; set; }
    public string? NombrePsicologo { get; set; }
    public DateTime? FechaCita { get; set; }
    public string? HoraInicio { get; set; }
    public string? HoraFin { get; set; }

    public bool DesdeCalendario => !string.IsNullOrEmpty(IdPsicologo);
}
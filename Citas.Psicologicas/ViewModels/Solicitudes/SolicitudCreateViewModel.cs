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
}

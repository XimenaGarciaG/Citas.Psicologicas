using System.ComponentModel.DataAnnotations;
using Citas.Psicologicas.DTOs.Usuarios;

namespace Citas.Psicologicas.ViewModels.Canalizaciones;

/// <summary>ViewModel para registrar una canalización</summary>
public class CanalizacionCreateViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un estudiante")]
    [Display(Name = "Estudiante")]
    public string IdEstudiante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    [Display(Name = "Motivo de Canalización")]
    [DataType(DataType.MultilineText)]
    public string Motivo { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    [DataType(DataType.MultilineText)]
    public string? Observaciones { get; set; }

    public string IdTutor { get; set; } = string.Empty;
    public List<UsuarioDto> Estudiantes { get; set; } = [];
}

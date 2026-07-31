using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Usuarios;

/// <summary>ViewModel para crear un nuevo usuario</summary>
public class UsuarioCreateViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo institucional es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [StringLength(200)]
    [Display(Name = "Correo Institucional")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "ESTUDIANTE";

    // Campos de Estudiante
    [Display(Name = "Matrícula / Número de control")]
    public string? Matricula { get; set; }

    [Display(Name = "Carrera")]
    public string? Carrera { get; set; }

    [Display(Name = "Cuatrimestre")]
    public int? Cuatrimestre { get; set; }

    [Display(Name = "Grupo")]
    public string? Grupo { get; set; }

    [Display(Name = "¿Es alumno regular?")]
    public bool EsRegular { get; set; } = true;

    // Campos de Tutor
    [Display(Name = "Departamento")]
    public string? Departamento { get; set; }

    // Campos de Psicólogo
    [Display(Name = "Cédula profesional")]
    public string? CedulaProfesional { get; set; }
}

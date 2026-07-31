using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Auth;

/// <summary>ViewModel para el formulario de registro de nuevos usuarios / creación de cuenta</summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo institucional es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
    [Display(Name = "Correo institucional")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    [Display(Name = "Tipo de cuenta")]
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

    public string? ErrorMessage { get; set; }
}

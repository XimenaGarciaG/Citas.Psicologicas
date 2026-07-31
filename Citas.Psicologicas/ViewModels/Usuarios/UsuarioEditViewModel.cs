using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Usuarios;

/// <summary>ViewModel para editar un usuario existente</summary>
public class UsuarioEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    [Display(Name = "Correo Institucional")]
    public string Correo { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;

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

    [Display(Name = "Departamento")]
    public string? Departamento { get; set; }

    [Display(Name = "Cédula profesional")]
    public string? CedulaProfesional { get; set; }

    [Display(Name = "Usuario Activo")]
    public bool Activo { get; set; }
}

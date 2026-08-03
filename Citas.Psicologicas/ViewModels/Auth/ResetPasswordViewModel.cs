using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Auth;

/// <summary>ViewModel para restablecer la contraseña</summary>
public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "El correo institucional es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
    [Display(Name = "Correo institucional")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El token de recuperación es obligatorio")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

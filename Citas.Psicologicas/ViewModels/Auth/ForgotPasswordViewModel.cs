using System.ComponentModel.DataAnnotations;

namespace Citas.Psicologicas.ViewModels.Auth;

/// <summary>ViewModel para la recuperación de contraseña</summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El correo institucional es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
    [Display(Name = "Correo institucional")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Enlace de recuperación generado (solo visible en modo respaldo local)</summary>
    public string? ResetLink { get; set; }

    public string? ErrorMessage { get; set; }
}

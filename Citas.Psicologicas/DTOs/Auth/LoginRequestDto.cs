namespace Citas.Psicologicas.DTOs.Auth;

/// <summary>DTO para la solicitud de login según especificación API REST</summary>
public class LoginRequestDto
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

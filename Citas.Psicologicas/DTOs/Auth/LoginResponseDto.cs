using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Auth;

/// <summary>DTO con la respuesta de login del servidor: { "token": "...", "rol": "ESTUDIANTE", "correo": "...", "idUsuario": 3 }</summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    
    [JsonPropertyName("idUsuario")]
    public object? IdUsuario { get; set; }
    
    public string? NombreCompleto { get; set; }
    public DateTime? Expiracion { get; set; }

    public string GetIdUsuarioString() => IdUsuario?.ToString() ?? "";
}

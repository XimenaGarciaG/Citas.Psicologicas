using Citas.Psicologicas.DTOs.Auth;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de autenticación y registro de usuarios</summary>
public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse<CreateUsuarioResponseDto>> RegisterAsync(CreateUsuarioDto dto);
    Task<bool> LogoutAsync(string token);
}

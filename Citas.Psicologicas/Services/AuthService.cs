using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Auth;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio de autenticación: Login, Register y Logout mediante la API</summary>
public class AuthService : BaseApiService, IAuthService
{
    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        => await PostAsync<LoginResponseDto>(ApiRoutes.Login, dto);

    /// <inheritdoc/>
    public async Task<ApiResponse<CreateUsuarioResponseDto>> RegisterAsync(CreateUsuarioDto dto)
        => await PostAsync<CreateUsuarioResponseDto>(ApiRoutes.Usuarios, dto);

    /// <inheritdoc/>
    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            var client = CreateClient(token);
            var response = await client.PostAsync(ApiRoutes.Logout, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error al invocar endpoint de logout");
            return false;
        }
    }
}

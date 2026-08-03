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
    {
        var result = await PostAsync<LoginResponseDto>(ApiRoutes.Login, dto);

        // La API responde HTTP 200 con { "mensaje": "..." } cuando las credenciales
        // son inválidas, por lo que se valida la presencia del token.
        if (result.Success && (result.Data is null || string.IsNullOrWhiteSpace(result.Data.Token)))
            return ApiResponseHelper.Fail<LoginResponseDto>("Credenciales incorrectas o usuario inactivo. Verifique su correo y contraseña.");

        return result;
    }

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

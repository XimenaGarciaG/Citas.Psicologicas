using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para gestión de usuarios del sistema</summary>
public class UsuarioService : BaseApiService, IUsuarioService
{
    public UsuarioService(IHttpClientFactory httpClientFactory, ILogger<UsuarioService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<UsuarioDto>>> GetAllAsync(string token)
        => await GetAsync<List<UsuarioDto>>(ApiRoutes.Usuarios, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<UsuarioDto>> GetByIdAsync(string id, string token)
        => await GetAsync<UsuarioDto>(string.Format(ApiRoutes.UsuarioById, id), token);

    /// <inheritdoc/>
    public async Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, string token)
        => await PostAsync<UsuarioDto>(ApiRoutes.Usuarios, dto, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<UsuarioDto>> UpdateAsync(string id, UpdateUsuarioDto dto, string token)
        => await PutAsync<UsuarioDto>(string.Format(ApiRoutes.UsuarioById, id), dto, token);
}

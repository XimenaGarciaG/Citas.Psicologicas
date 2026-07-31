using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Canalizaciones;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para gestión de canalizaciones por tutores</summary>
public class CanalizacionService : BaseApiService, ICanalizacionService
{
    public CanalizacionService(IHttpClientFactory httpClientFactory, ILogger<CanalizacionService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CanalizacionDto>>> GetAllAsync(string token)
        => await GetAsync<List<CanalizacionDto>>(ApiRoutes.Canalizaciones, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<CanalizacionDto>> CreateAsync(CreateCanalizacionDto dto, string token)
        => await PostAsync<CanalizacionDto>(ApiRoutes.Canalizaciones, dto, token);
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para gestión integral de citas psicológicas</summary>
public class CitaService : BaseApiService, ICitaService
{
    public CitaService(IHttpClientFactory httpClientFactory, ILogger<CitaService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CitaDto>>> GetAllAsync(string token)
        => await GetAsync<List<CitaDto>>(ApiRoutes.Citas, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<CitaDto>> GetByIdAsync(string id, string token)
        => await GetAsync<CitaDto>(string.Format(ApiRoutes.CitaById, id), token);

    /// <inheritdoc/>
    public async Task<ApiResponse<CitaDto>> CreateAsync(CreateCitaDto dto, string token)
        => await PostAsync<CitaDto>(ApiRoutes.Citas, dto, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<CitaDto>> UpdateAsync(string id, UpdateCitaDto dto, string token)
        => await PutAsync<CitaDto>(string.Format(ApiRoutes.CitaById, id), dto, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> ConfirmarAsync(string id, string token)
        => await PatchAsync<bool>(string.Format(ApiRoutes.CitaConfirmar, id), null, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> CancelarAsync(string id, string token)
        => await PatchAsync<bool>(string.Format(ApiRoutes.CitaCancelar, id), null, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<CitaDto>> ReagendarAsync(string id, ReagendarCitaDto dto, string token)
        => await PatchAsync<CitaDto>(string.Format(ApiRoutes.CitaReagendar, id), dto, token);
}

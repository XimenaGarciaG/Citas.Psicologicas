using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para gestión de solicitudes de atención psicológica</summary>
public class SolicitudService : BaseApiService, ISolicitudService
{
    public SolicitudService(IHttpClientFactory httpClientFactory, ILogger<SolicitudService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<SolicitudDto>>> GetAllAsync(string token)
        => await GetAsync<List<SolicitudDto>>(ApiRoutes.Solicitudes, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<SolicitudDto>> GetByIdAsync(string id, string token)
        => await GetAsync<SolicitudDto>(string.Format(ApiRoutes.SolicitudById, id), token);

    /// <inheritdoc/>
    public async Task<ApiResponse<SolicitudDto>> CreateAsync(CreateSolicitudDto dto, string token)
        => await PostAsync<SolicitudDto>(ApiRoutes.Solicitudes, dto, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<SolicitudDto>> UpdatePrioridadAsync(string id, UpdatePrioridadDto dto, string token)
        => await PutAsync<SolicitudDto>(string.Format(ApiRoutes.SolicitudPrioridad, id), dto, token);
}

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
    public async Task<ApiResponse<List<SolicitudDto>>> GetAllAsync(string token, string? estado = null)
    {
        var url = string.IsNullOrEmpty(estado) 
            ? ApiRoutes.Solicitudes 
            : $"{ApiRoutes.Solicitudes}?estado={Uri.EscapeDataString(estado)}";
        return await GetAsync<List<SolicitudDto>>(url, token);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SolicitudDto>> GetByIdAsync(string id, string token)
        => await GetAsync<SolicitudDto>(string.Format(ApiRoutes.SolicitudById, id), token);

    /// <inheritdoc/>
    public async Task<ApiResponse<SolicitudDto>> CreateAsync(CreateSolicitudDto dto, string token)
        => await PostAsync<SolicitudDto>(ApiRoutes.Solicitudes, dto, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<UpdatePrioridadResponseDto>> UpdatePrioridadAsync(string id, string token)
        => await PutAsync<UpdatePrioridadResponseDto>(string.Format(ApiRoutes.SolicitudPrioridad, id), new { }, token);
}

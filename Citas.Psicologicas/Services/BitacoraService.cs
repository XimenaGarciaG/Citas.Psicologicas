using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Bitacora;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para registro de asistencia en bitácora administrativa</summary>
public class BitacoraService : BaseApiService, IBitacoraService
{
    public BitacoraService(IHttpClientFactory httpClientFactory, ILogger<BitacoraService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<BitacoraDto>>> GetAllAsync(string token)
        => await GetAsync<List<BitacoraDto>>(ApiRoutes.Bitacora, token);

    /// <inheritdoc/>
    public async Task<ApiResponse<BitacoraDto>> CreateAsync(CreateBitacoraDto dto, string token)
        => await PostAsync<BitacoraDto>(ApiRoutes.Bitacora, dto, token);
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Dashboard;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para obtener estadísticas del dashboard</summary>
public class DashboardService : BaseApiService, IDashboardService
{
    public DashboardService(IHttpClientFactory httpClientFactory, ILogger<DashboardService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync(string token)
        => await GetAsync<DashboardDto>(ApiRoutes.Dashboard, token);
}

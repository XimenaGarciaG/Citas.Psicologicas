using Citas.Psicologicas.DTOs.Dashboard;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de estadísticas del dashboard</summary>
public interface IDashboardService
{
    Task<ApiResponse<DashboardDto>> GetDashboardAsync(string token);
}

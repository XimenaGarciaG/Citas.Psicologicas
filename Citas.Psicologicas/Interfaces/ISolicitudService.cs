using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de solicitudes</summary>
public interface ISolicitudService
{
    Task<ApiResponse<List<SolicitudDto>>> GetAllAsync(string token);
    Task<ApiResponse<SolicitudDto>> GetByIdAsync(string id, string token);
    Task<ApiResponse<SolicitudDto>> CreateAsync(CreateSolicitudDto dto, string token);
    Task<ApiResponse<SolicitudDto>> UpdatePrioridadAsync(string id, UpdatePrioridadDto dto, string token);
}

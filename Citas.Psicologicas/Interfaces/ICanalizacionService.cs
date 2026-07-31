using Citas.Psicologicas.DTOs.Canalizaciones;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de canalizaciones</summary>
public interface ICanalizacionService
{
    Task<ApiResponse<List<CanalizacionDto>>> GetAllAsync(string token);
    Task<ApiResponse<CanalizacionDto>> CreateAsync(CreateCanalizacionDto dto, string token);
}

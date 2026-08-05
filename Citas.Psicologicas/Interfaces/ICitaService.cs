using Citas.Psicologicas.DTOs.Citas;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de citas</summary>
public interface ICitaService
{
    Task<ApiResponse<List<CitaDto>>> GetAllAsync(string token);
    Task<ApiResponse<CitaDto>> GetByIdAsync(string id, string token);
    Task<ApiResponse<CitaDto>> CreateAsync(CreateCitaDto dto, string token);
    Task<ApiResponse<CitaDto>> UpdateAsync(string id, UpdateCitaDto dto, string token);
    Task<ApiResponse<bool>> ConfirmarAsync(string id, string token);
    Task<ApiResponse<bool>> CancelarAsync(string id, string token);
    Task<ApiResponse<CitaDto>> ReagendarAsync(string id, ReagendarCitaDto dto, string token);
}

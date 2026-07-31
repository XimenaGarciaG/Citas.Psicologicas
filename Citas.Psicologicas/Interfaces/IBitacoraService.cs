using Citas.Psicologicas.DTOs.Bitacora;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de bitácora</summary>
public interface IBitacoraService
{
    Task<ApiResponse<List<BitacoraDto>>> GetAllAsync(string token);
    Task<ApiResponse<BitacoraDto>> CreateAsync(CreateBitacoraDto dto, string token);
}

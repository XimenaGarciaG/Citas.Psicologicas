using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de usuarios</summary>
public interface IUsuarioService
{
    Task<ApiResponse<List<UsuarioDto>>> GetAllAsync(string token);
    Task<ApiResponse<UsuarioDto>> GetByIdAsync(string id, string token);
    Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, string token);
    Task<ApiResponse<UsuarioDto>> UpdateAsync(string id, UpdateUsuarioDto dto, string token);
}

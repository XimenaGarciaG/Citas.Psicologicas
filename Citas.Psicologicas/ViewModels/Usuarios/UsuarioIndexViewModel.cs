using Citas.Psicologicas.DTOs.Usuarios;

namespace Citas.Psicologicas.ViewModels.Usuarios;

/// <summary>ViewModel para el listado de usuarios</summary>
public class UsuarioIndexViewModel
{
    public List<UsuarioDto> Usuarios { get; set; } = [];
    public string? FiltroBusqueda { get; set; }
    public string? FiltroRol { get; set; }
    public string? FiltroEstado { get; set; }
    public int TotalRegistros => Usuarios.Count;
}

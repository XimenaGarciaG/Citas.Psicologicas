using Citas.Psicologicas.Constants;

namespace Citas.Psicologicas.ViewModels.Perfil;

/// <summary>ViewModel con los datos personales del usuario autenticado</summary>
public class PerfilViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime? FechaCreacion { get; set; }

    public string? Matricula { get; set; }
    public string? Carrera { get; set; }
    public int? Cuatrimestre { get; set; }
    public string? Grupo { get; set; }
    public bool? EsRegular { get; set; }
    public string? Departamento { get; set; }
    public string? CedulaProfesional { get; set; }

    public bool Activo { get; set; } = true;

    public string RolLabel => Roles.GetLabel(Rol);
    public string Iniciales
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto)) return "U";
            var parts = NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : $"{parts[0][0]}".ToUpper();
        }
    }
}

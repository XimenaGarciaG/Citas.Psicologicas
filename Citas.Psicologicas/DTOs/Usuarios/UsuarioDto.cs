using System.Text.Json.Serialization;
using Citas.Psicologicas.Constants;

namespace Citas.Psicologicas.DTOs.Usuarios;

/// <summary>DTO de lectura para usuarios devuelto por la API REST</summary>
public class UsuarioDto
{
    [JsonPropertyName("idUsuario")]
    public object? IdUsuario { get; set; }

    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estatus { get; set; } = "ACTIVO";
    public DateTime? FechaCreacion { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }

    // Campos detallados (GET /api/Usuarios/{id})
    public string? Matricula { get; set; }
    public string? Carrera { get; set; }
    public int? Cuatrimestre { get; set; }
    public string? Grupo { get; set; }
    public bool? EsRegular { get; set; }
    public string? Departamento { get; set; }
    public string? CedulaProfesional { get; set; }

    public string Id => IdUsuario?.ToString() ?? "";
    public string Nombre => NombreCompleto;
    public bool Activo => Estatus?.ToUpper() is "ACTIVO" or "TRUE" or "ACTIVE";
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

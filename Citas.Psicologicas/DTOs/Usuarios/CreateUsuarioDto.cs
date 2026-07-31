namespace Citas.Psicologicas.DTOs.Usuarios;

/// <summary>DTO para creación / registro de usuario en POST /api/Usuarios</summary>
public class CreateUsuarioDto
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = "ESTUDIANTE";
    public string NombreCompleto { get; set; } = string.Empty;

    // ESTUDIANTE
    public string? Matricula { get; set; }
    public string? Carrera { get; set; }
    public int? Cuatrimestre { get; set; }
    public string? Grupo { get; set; }
    public bool? EsRegular { get; set; }

    // TUTOR
    public string? Departamento { get; set; }

    // PSICOLOGO
    public string? CedulaProfesional { get; set; }
}

/// <summary>Respuesta al crear un usuario</summary>
public class CreateUsuarioResponseDto
{
    public string? Mensaje { get; set; }
    public object? IdUsuario { get; set; }
}

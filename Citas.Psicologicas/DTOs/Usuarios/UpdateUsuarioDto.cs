namespace Citas.Psicologicas.DTOs.Usuarios;

/// <summary>DTO para actualizar datos de usuario en PUT /api/Usuarios/{id}</summary>
public class UpdateUsuarioDto
{
    public string Correo { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Matricula { get; set; }
    public string? Carrera { get; set; }
    public int? Cuatrimestre { get; set; }
    public string? Grupo { get; set; }
    public bool? EsRegular { get; set; }
    public string? Departamento { get; set; }
    public string? CedulaProfesional { get; set; }
}

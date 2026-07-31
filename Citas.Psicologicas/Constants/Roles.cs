namespace Citas.Psicologicas.Constants;

/// <summary>Constantes de roles del sistema alineadas con la API REST</summary>
public static class Roles
{
    public const string Administrador = "ADMIN";
    public const string Psicologo = "PSICOLOGO";
    public const string Tutor = "TUTOR";
    public const string Estudiante = "ESTUDIANTE";

    public static readonly string[] All = [Administrador, Psicologo, Tutor, Estudiante];
    public static readonly string[] StaffRoles = [Administrador, Psicologo];

    /// <summary>Obtiene una etiqueta legible para la interfaz de usuario</summary>
    public static string GetLabel(string? rol) => rol?.ToUpper() switch
    {
        "ADMIN" => "Administrador",
        "PSICOLOGO" => "Psicólogo/a",
        "TUTOR" => "Tutor/a",
        "ESTUDIANTE" => "Estudiante",
        _ => rol ?? "Usuario"
    };
}

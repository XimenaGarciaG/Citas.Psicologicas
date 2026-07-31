using Citas.Psicologicas.Constants;

namespace Citas.Psicologicas.Helpers;

/// <summary>Helper para manejo de sesión de usuario</summary>
public static class SessionHelper
{
    public static string? GetToken(ISession session) => session.GetString(SessionKeys.Token);
    public static string? GetRol(ISession session) => session.GetString(SessionKeys.Rol);
    public static string? GetCorreo(ISession session) => session.GetString(SessionKeys.Correo);
    public static string? GetIdUsuario(ISession session) => session.GetString(SessionKeys.IdUsuario);
    public static string? GetNombreCompleto(ISession session) => session.GetString(SessionKeys.NombreCompleto);

    public static bool IsAuthenticated(ISession session) =>
        !string.IsNullOrEmpty(GetToken(session));

    public static bool HasRole(ISession session, params string[] roles) =>
        roles.Contains(GetRol(session));

    public static void SetSession(
        ISession session,
        string token,
        string rol,
        string correo,
        string idUsuario,
        string nombreCompleto)
    {
        session.SetString(SessionKeys.Token, token);
        session.SetString(SessionKeys.Rol, rol);
        session.SetString(SessionKeys.Correo, correo);
        session.SetString(SessionKeys.IdUsuario, idUsuario);
        session.SetString(SessionKeys.NombreCompleto, nombreCompleto);
    }

    public static void ClearSession(ISession session) => session.Clear();
}

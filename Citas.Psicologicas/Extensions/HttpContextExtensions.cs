using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Extensions;

/// <summary>Extensiones de HttpContext para sesión y roles</summary>
public static class HttpContextExtensions
{
    public static string? GetUserToken(this HttpContext context) =>
        SessionHelper.GetToken(context.Session);

    public static string? GetUserRole(this HttpContext context) =>
        SessionHelper.GetRol(context.Session);

    public static string? GetUserId(this HttpContext context) =>
        SessionHelper.GetIdUsuario(context.Session);

    public static string? GetUserName(this HttpContext context) =>
        SessionHelper.GetNombreCompleto(context.Session);

    public static bool IsAuthenticated(this HttpContext context) =>
        SessionHelper.IsAuthenticated(context.Session);

    public static bool IsInRole(this HttpContext context, params string[] roles) =>
        SessionHelper.HasRole(context.Session, roles);
}

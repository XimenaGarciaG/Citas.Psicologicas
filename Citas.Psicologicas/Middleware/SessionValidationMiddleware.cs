using System.Globalization;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Middleware;

/// <summary>Middleware que valida la sesión activa del usuario en cada request</summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] PublicPaths =
    [
        "/Auth/Login",
        "/Auth/Register",
        "/Auth/ForgotPassword",
        "/Auth/ResetPassword",
        "/Auth/Logout",
        "/Error",
        "/AccessDenied"
    ];

    private static readonly string[] StaticExtensions =
    [
        ".css", ".js", ".jpg", ".jpeg", ".png", ".gif",
        ".svg", ".ico", ".woff", ".woff2", ".ttf", ".map"
    ];

    public SessionValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Permitir archivos estáticos
        if (StaticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Permitir rutas públicas
        if (PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Verificar autenticación
        if (!SessionHelper.IsAuthenticated(context.Session))
        {
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Auth/Login?returnUrl={returnUrl}");
            return;
        }

        // Expiración absoluta de la sesión
        var expiraSesion = context.Session.GetString(SessionKeys.ExpiraSesion);
        if (!string.IsNullOrEmpty(expiraSesion) &&
            DateTime.TryParse(
                expiraSesion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var limite) &&
            limite <= DateTime.Now)
        {
            context.Session.Clear();
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Auth/Login?returnUrl={returnUrl}");
            return;
        }

        await _next(context);
    }
}

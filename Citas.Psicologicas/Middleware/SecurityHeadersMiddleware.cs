namespace Citas.Psicologicas.Middleware;

/// <summary>
/// Middleware que agrega cabeceras de seguridad HTTP a todas las respuestas:
/// Content-Security-Policy, X-Content-Type-Options, X-Frame-Options,
/// Referrer-Policy y Permissions-Policy.
/// </summary>
public class SecurityHeadersMiddleware
{
    // Los 'unsafe-inline' son necesarios por los scripts/estilos inline y los
    // CDNs por los recursos cargados en _Layout y vistas.
    private const string ContentSecurityPolicy =
        "default-src 'self';" +
        " script-src 'self' 'unsafe-inline' https://code.jquery.com https://cdn.jsdelivr.net https://cdn.datatables.net https://cdnjs.cloudflare.com;" +
        " style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdn.datatables.net https://cdnjs.cloudflare.com;" +
        " img-src 'self' data: https:;" +
        " font-src 'self' data: https://cdnjs.cloudflare.com;" +
        " connect-src 'self';" +
        " frame-ancestors 'self';" +
        " base-uri 'self';" +
        " form-action 'self';" +
        " object-src 'none'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["Content-Security-Policy"] = ContentSecurityPolicy;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), usb=(), payment=(), interest-cohort=()";

        await _next(context);
    }
}

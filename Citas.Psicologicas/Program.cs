using System.Threading.RateLimiting;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Extensions;
using Citas.Psicologicas.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ─── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Demasiados intentos. Espere unos minutos e intente nuevamente.",
            cancellationToken);
    };

    // Política para endpoints de autenticación (login, registro, recuperación).
    options.AddPolicy("Auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: AuthPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

static string AuthPartitionKey(HttpContext ctx)
{
    var userId = ctx.Session.GetString(SessionKeys.IdUsuario);
    return string.IsNullOrEmpty(userId)
        ? ctx.Connection.RemoteIpAddress?.ToString() ?? "anonimo"
        : $"u:{userId}";
}

// ─── HttpContext Accessor ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ─── Sesión ───────────────────────────────────────────────────────────────────
builder.Services.AddSessionConfiguration(builder.Configuration);

// ─── Servicios de la aplicación + HttpClientFactory ───────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ─── Pipeline ─────────────────────────────────────────────────────────────────
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/ServerError");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseRateLimiter();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

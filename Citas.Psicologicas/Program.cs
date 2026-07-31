using Citas.Psicologicas.Extensions;
using Citas.Psicologicas.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ─── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── HttpContext Accessor ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ─── Sesión ───────────────────────────────────────────────────────────────────
builder.Services.AddSessionConfiguration(builder.Configuration);

// ─── Servicios de la aplicación + HttpClientFactory ───────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ─── Pipeline ─────────────────────────────────────────────────────────────────
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
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

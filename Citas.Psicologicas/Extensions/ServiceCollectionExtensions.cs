using Citas.Psicologicas.Configuration;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Services;

namespace Citas.Psicologicas.Extensions;

/// <summary>Extensiones para registro de servicios en DI</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>()
            ?? throw new InvalidOperationException("ApiSettings no configurado.");

        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ISolicitudService, SolicitudService>();
        services.AddScoped<ICanalizacionService, CanalizacionService>();
        services.AddScoped<ICitaService, CitaService>();
        services.AddScoped<IBitacoraService, BitacoraService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<INotificacionService, NotificacionService>();

        services.AddScoped<ILocalDataService, LocalDataService>();

        return services;
    }

    public static IServiceCollection AddSessionConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sessionSettings = configuration.GetSection("Session").Get<SessionSettings>()
            ?? new SessionSettings();

        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(sessionSettings.TimeoutMinutes);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = sessionSettings.CookieName;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        return services;
    }
}

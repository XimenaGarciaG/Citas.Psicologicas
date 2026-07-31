namespace Citas.Psicologicas.Configuration;

/// <summary>Configuración fuertemente tipada de la aplicación</summary>
public class AppSettings
{
    public ApiSettings ApiSettings { get; set; } = new();
    public SessionSettings Session { get; set; } = new();
}

/// <summary>Configuración de la API REST</summary>
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>Configuración de sesión de usuario</summary>
public class SessionSettings
{
    public int TimeoutMinutes { get; set; } = 60;
    public string CookieName { get; set; } = ".Citas.Session";
}

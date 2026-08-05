using System.Text.Json;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Services;

/// <summary>
/// Implementación de <see cref="ILocalDataService"/> basada en archivos JSON.
/// Almacena la información bajo la carpeta AppData del proyecto. Es un respaldo
/// local para las funciones que la API REST hosteada aún no expone.
/// </summary>
public class LocalDataService : ILocalDataService
{
    private static readonly object Sync = new();

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataPath;

    public LocalDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "AppData");
        Directory.CreateDirectory(_dataPath);
        EnsureSeed();
    }

    private string FileFor(string name) => Path.Combine(_dataPath, $"{name}.json");

    private T Load<T>(string name, T fallback) where T : class
    {
        lock (Sync)
        {
            var file = FileFor(name);
            if (!File.Exists(file))
                return fallback;

            try
            {
                var json = File.ReadAllText(file);
                return JsonSerializer.Deserialize<T>(json, ReadOptions) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }

    private void Save<T>(string name, T data)
    {
        lock (Sync)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileFor(name), json);
        }
    }

    // ─── Catálogos ────────────────────────────────────────────────────────────

    public List<Carrera> GetCarreras() => Load("carreras", SeedCarreras());

    public void SaveCarreras(List<Carrera> carreras) => Save("carreras", carreras);

    public List<Grupo> GetGrupos() => Load("grupos", SeedGrupos());

    public void SaveGrupos(List<Grupo> grupos) => Save("grupos", grupos);

    // ─── Configuración ─────────────────────────────────────────────────────────

    public ConfiguracionSistema GetConfiguracion() => Load("configuracion", new ConfiguracionSistema());

    public void SaveConfiguracion(ConfiguracionSistema config) => Save("configuracion", config);

    // ─── Notificaciones ────────────────────────────────────────────────────────

    public List<NotificacionRegistro> GetNotificaciones() => Load("notificaciones", new List<NotificacionRegistro>());

    public void AddNotificacion(NotificacionRegistro notificacion)
    {
        var items = GetNotificaciones();
        notificacion.Id = items.Count == 0 ? 1 : items.Max(n => n.Id) + 1;
        items.Add(notificacion);
        Save("notificaciones", items);
    }

    public void MarcarNotificacionEnviada(int id)
    {
        var items = GetNotificaciones();
        var idx = items.FindIndex(n => n.Id == id);
        if (idx >= 0)
        {
            items[idx].Fecha = DateTime.Now;
            Save("notificaciones", items);
        }
    }

    // ─── Seguimientos ──────────────────────────────────────────────────────────

    public List<SeguimientoRegistro> GetSeguimientos() => Load("seguimientos", new List<SeguimientoRegistro>());

    public void AddSeguimiento(SeguimientoRegistro seguimiento)
    {
        var items = GetSeguimientos();
        seguimiento.Id = items.Count == 0 ? 1 : items.Max(s => s.Id) + 1;
        items.Add(seguimiento);
        Save("seguimientos", items);
    }

    public void UpdateSeguimiento(SeguimientoRegistro seguimiento)
    {
        var items = GetSeguimientos();
        var idx = items.FindIndex(s => s.Id == seguimiento.Id);
        if (idx >= 0)
        {
            items[idx] = seguimiento;
            Save("seguimientos", items);
        }
    }

    // ─── Bitácora pendiente de confirmación ──────────────────────────────────

    public List<BitacoraPendiente> GetBitacorasPendientes()
        => Load("bitacora_pendiente", new List<BitacoraPendiente>());

    public BitacoraPendiente? GetBitacoraPendiente(string idCita)
        => GetBitacorasPendientes().FirstOrDefault(b => b.IdCita == idCita);

    public void AddBitacoraPendiente(BitacoraPendiente bitacora)
    {
        var items = GetBitacorasPendientes();
        bitacora.Id = items.Count == 0 ? 1 : items.Max(b => b.Id) + 1;
        items.Add(bitacora);
        Save("bitacora_pendiente", items);
    }

    public void UpdateBitacoraPendiente(BitacoraPendiente bitacora)
    {
        var items = GetBitacorasPendientes();
        var idx = items.FindIndex(b => b.Id == bitacora.Id);
        if (idx >= 0)
        {
            items[idx] = bitacora;
            Save("bitacora_pendiente", items);
        }
    }

    // ─── Confirmaciones de asistencia ──────────────────────────────────────────

    public List<ConfirmacionAsistencia> GetConfirmaciones() => Load("confirmaciones", new List<ConfirmacionAsistencia>());

    public ConfirmacionAsistencia? GetConfirmacion(string idCita)
        => GetConfirmaciones().FirstOrDefault(c => c.IdCita == idCita);

    public void SetConfirmacion(ConfirmacionAsistencia confirmacion)
    {
        var items = GetConfirmaciones();
        var idx = items.FindIndex(c => c.IdCita == confirmacion.IdCita);
        if (idx >= 0)
            items[idx] = confirmacion;
        else
            items.Add(confirmacion);
        Save("confirmaciones", items);
    }

    // ─── Reagendas de citas (una por cita) ────────────────────────────────────

    public List<ReagendaRegistro> GetReagendas() => Load("reagendas", new List<ReagendaRegistro>());

    public ReagendaRegistro? GetReagenda(string idCita)
        => GetReagendas().FirstOrDefault(r => string.Equals(r.IdCita, idCita, StringComparison.OrdinalIgnoreCase));

    public void AddReagenda(ReagendaRegistro reagenda)
    {
        var items = GetReagendas();
        var existing = items.FindIndex(r => string.Equals(r.IdCita, reagenda.IdCita, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0)
            items[existing] = reagenda;
        else
        {
            reagenda.Id = items.Count == 0 ? 1 : items.Max(r => r.Id) + 1;
            items.Add(reagenda);
        }
        Save("reagendas", items);
    }

    // ─── Bloqueos de disponibilidad de psicólogas ───────────────────────────────

    public List<BloqueoDisponibilidad> GetBloqueos() => Load("disponibilidad", new List<BloqueoDisponibilidad>());

    public List<BloqueoDisponibilidad> GetBloqueos(DateTime fecha)
        => GetBloqueos().Where(b => b.Fecha.Date == fecha.Date).ToList();

    public void AddBloqueo(BloqueoDisponibilidad bloqueo)
    {
        var items = GetBloqueos();
        bloqueo.Id = items.Count == 0 ? 1 : items.Max(b => b.Id) + 1;
        items.Add(bloqueo);
        Save("disponibilidad", items);
    }

    public void RemoveBloqueo(int id)
    {
        var items = GetBloqueos();
        items.RemoveAll(b => b.Id == id);
        Save("disponibilidad", items);
    }

    public bool TieneBloqueo(string idPsicologo, DateTime fecha, string horaInicio, string horaFin)
    {
        var inicio = TimeSpan.Parse(horaInicio);
        var fin = TimeSpan.Parse(horaFin);
        return GetBloqueos(fecha).Any(b =>
            string.Equals(b.IdPsicologo, idPsicologo, StringComparison.OrdinalIgnoreCase) &&
            TimeSpan.Parse(b.HoraInicio) < fin &&
            TimeSpan.Parse(b.HoraFin) > inicio);
    }

    // ─── Solicitudes generadas desde el calendario ──────────────────────────────

    public List<SolicitudCalendario> GetSolicitudesCalendario() => Load("solicitudes_calendario", new List<SolicitudCalendario>());

    public List<SolicitudCalendario> GetSolicitudesCalendarioPendientes()
        => GetSolicitudesCalendario().Where(s => !s.Atendida).OrderBy(s => s.FechaCita).ToList();

    public void AddSolicitudCalendario(SolicitudCalendario solicitud)
    {
        var items = GetSolicitudesCalendario();
        solicitud.Id = items.Count == 0 ? 1 : items.Max(s => s.Id) + 1;
        items.Add(solicitud);
        Save("solicitudes_calendario", items);
    }

    public void MarcarSolicitudCalendarioAtendida(int id)
    {
        var items = GetSolicitudesCalendario();
        var idx = items.FindIndex(s => s.Id == id);
        if (idx >= 0)
        {
            items[idx].Atendida = true;
            Save("solicitudes_calendario", items);
        }
    }

    // ─── Vínculos canalización → solicitud ──────────────────────────────────

    public List<CanalizacionSolicitud> GetCanalizacionesSolicitudes()
        => Load("canalizaciones_solicitud", new List<CanalizacionSolicitud>());

    public void AddCanalizacionSolicitud(CanalizacionSolicitud vinculo)
    {
        var items = GetCanalizacionesSolicitudes();
        vinculo.Id = items.Count == 0 ? 1 : items.Max(v => v.Id) + 1;
        items.Add(vinculo);
        Save("canalizaciones_solicitud", items);
    }

    // ─── Psicóloga Encargada (usuario designado) ─────────────────────────────

    public string? GetPsicologaEncargadaId()
        => Load("psicologa_encargada", new PsicologaEncargada()).IdUsuario;

    public void SetPsicologaEncargadaId(string? idUsuario)
        => Save("psicologa_encargada", new PsicologaEncargada { IdUsuario = idUsuario ?? string.Empty });

    // ─── Usuarios locales (contraseñas y estado) ───────────────────────────────

    private List<UsuarioLocal> GetUsuariosLocales() => Load("usuarios_local", new List<UsuarioLocal>());

    public void SetContrasenaLocal(string userId, string password)
    {
        var items = GetUsuariosLocales();
        var item = items.FirstOrDefault(u => u.Id == userId);
        if (item is null)
        {
            items.Add(new UsuarioLocal { Id = userId, Password = password });
        }
        else
        {
            item.Password = password;
        }
        Save("usuarios_local", items);
    }

    public string? GetContrasenaLocal(string userId)
        => GetUsuariosLocales().FirstOrDefault(u => u.Id == userId)?.Password;

    public void SetContrasenaLocalPorCorreo(string correo, string password)
    {
        var items = GetUsuariosLocales();
        var item = items.FirstOrDefault(u => string.Equals(u.Email, correo, StringComparison.OrdinalIgnoreCase));
        if (item is null)
        {
            items.Add(new UsuarioLocal { Email = correo, Password = password });
        }
        else
        {
            item.Password = password;
        }
        Save("usuarios_local", items);
    }

    public string? GetContrasenaLocalPorCorreo(string correo)
        => GetUsuariosLocales().FirstOrDefault(u => string.Equals(u.Email, correo, StringComparison.OrdinalIgnoreCase))?.Password;

    public void SetUsuarioActivoLocal(string userId, bool activo)
    {
        var items = GetUsuariosLocales();
        var item = items.FirstOrDefault(u => u.Id == userId);
        if (item is null)
        {
            items.Add(new UsuarioLocal { Id = userId, Activo = activo });
        }
        else
        {
            item.Activo = activo;
        }
        Save("usuarios_local", items);
    }

    public bool? GetUsuarioActivoLocal(string userId)
        => GetUsuariosLocales().FirstOrDefault(u => u.Id == userId)?.Activo;

    // ─── Tokens de recuperación ────────────────────────────────────────────────

    private List<ResetTokenRecord> GetResetTokens() => Load("reset_tokens", new List<ResetTokenRecord>());

    public void SetResetToken(string email, string token)
    {
        var items = GetResetTokens();
        var existing = items.FirstOrDefault(t => string.Equals(t.Email, email, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
            items.Add(new ResetTokenRecord { Email = email, Token = token });
        else
            existing.Token = token;
        Save("reset_tokens", items);
    }

    public string? GetResetToken(string email)
        => GetResetTokens().FirstOrDefault(t => string.Equals(t.Email, email, StringComparison.OrdinalIgnoreCase))?.Token;

    public void RemoveResetToken(string email)
    {
        var items = GetResetTokens();
        items.RemoveAll(t => string.Equals(t.Email, email, StringComparison.OrdinalIgnoreCase));
        Save("reset_tokens", items);
    }

    // ─── Semilla inicial ───────────────────────────────────────────────────────

    private void EnsureSeed()
    {
        if (!File.Exists(FileFor("carreras")))
            SaveCarreras(SeedCarreras());
        if (!File.Exists(FileFor("grupos")))
            SaveGrupos(SeedGrupos());
        if (!File.Exists(FileFor("configuracion")))
            SaveConfiguracion(new ConfiguracionSistema());
        if (!File.Exists(FileFor("notificaciones")))
            Save("notificaciones", new List<NotificacionRegistro>());
        if (!File.Exists(FileFor("seguimientos")))
            Save("seguimientos", new List<SeguimientoRegistro>());
        if (!File.Exists(FileFor("bitacora_pendiente")))
            Save("bitacora_pendiente", new List<BitacoraPendiente>());
        if (!File.Exists(FileFor("confirmaciones")))
            Save("confirmaciones", new List<ConfirmacionAsistencia>());
        if (!File.Exists(FileFor("reagendas")))
            Save("reagendas", new List<ReagendaRegistro>());
        if (!File.Exists(FileFor("disponibilidad")))
            Save("disponibilidad", new List<BloqueoDisponibilidad>());
        if (!File.Exists(FileFor("solicitudes_calendario")))
            Save("solicitudes_calendario", new List<SolicitudCalendario>());
        if (!File.Exists(FileFor("canalizaciones_solicitud")))
            Save("canalizaciones_solicitud", new List<CanalizacionSolicitud>());
        if (!File.Exists(FileFor("psicologa_encargada")))
            Save("psicologa_encargada", new PsicologaEncargada());
        if (!File.Exists(FileFor("usuarios_local")))
            Save("usuarios_local", new List<UsuarioLocal>());
        if (!File.Exists(FileFor("reset_tokens")))
            Save("reset_tokens", new List<ResetTokenRecord>());
    }

    private static List<Carrera> SeedCarreras() =>
    [
        new() { Id = 1, Nombre = "Ingeniería en Desarrollo de Software" },
        new() { Id = 2, Nombre = "Ingeniería en Tecnologías de la Información" },
        new() { Id = 3, Nombre = "Ingeniería Industrial" },
        new() { Id = 4, Nombre = "Ingeniería en Mecatrónica" },
        new() { Id = 5, Nombre = "Ingeniería en Logística" },
        new() { Id = 6, Nombre = "Licenciatura en Administración" },
        new() { Id = 7, Nombre = "Licenciatura en Contaduría Pública" },
        new() { Id = 8, Nombre = "Licenciatura en Terapia Física" },
        new() { Id = 9, Nombre = "Licenciatura en Psicología" }
    ];

    private static List<Grupo> SeedGrupos() =>
    [
        new() { Id = 1, Nombre = "9IDGS-G1" },
        new() { Id = 2, Nombre = "9IDGS-G2" },
        new() { Id = 3, Nombre = "8IDS" },
        new() { Id = 4, Nombre = "7ITI-G1" },
        new() { Id = 5, Nombre = "6INI" },
        new() { Id = 6, Nombre = "5LADM" }
    ];

    private class UsuarioLocal
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool? Activo { get; set; }
    }

    private class ResetTokenRecord
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

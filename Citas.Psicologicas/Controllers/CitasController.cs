using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Citas;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de agenda y gestión de citas psicológicas</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor, Roles.Estudiante)]
public class CitasController : Controller
{
    private readonly ICitaService _citaService;
    private readonly ISolicitudService _solicitudService;
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<CitasController> _logger;

    public CitasController(
        ICitaService citaService,
        ISolicitudService solicitudService,
        IUsuarioService usuarioService,
        ILogger<CitasController> logger)
    {
        _citaService = citaService;
        _solicitudService = solicitudService;
        _usuarioService = usuarioService;
        _logger = logger;
    }

    // GET: /Citas
    public async Task<IActionResult> Index(string? estado, DateTime? inicio, DateTime? fin, string? busqueda, string vista = "Lista")
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.GetAllAsync(token);

        var citas = result.Data ?? [];

        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);

        // Filtrar según rol
        if (rol == Roles.Estudiante)
            citas = citas.Where(c => string.Equals(c.IdEstudiante?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();
        else if (rol == Roles.Psicologo)
            citas = citas.Where(c => string.Equals(c.IdPsicologo?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(estado))
            citas = citas.Where(c => string.Equals(c.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

        if (inicio.HasValue)
            citas = citas.Where(c => c.Fecha >= inicio.Value).ToList();

        if (fin.HasValue)
            citas = citas.Where(c => c.Fecha <= fin.Value).ToList();

        if (!string.IsNullOrEmpty(busqueda))
            citas = citas.Where(c =>
                (c.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.NombrePsicologo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        var vm = new CitaIndexViewModel
        {
            Citas = citas,
            FiltroEstado = estado,
            FiltroFechaInicio = inicio,
            FiltroFechaFin = fin,
            FiltroBusqueda = busqueda,
            Vista = vista
        };

        ViewBag.PageTitle = "Agenda de Citas";
        ViewBag.CitasJson = System.Text.Json.JsonSerializer.Serialize(citas,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        return View(vm);
    }

    // GET: /Citas/Create
    [AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
    public async Task<IActionResult> Create()
    {
        var vm = await CargarDatosCitaAsync();
        ViewBag.PageTitle = "Agendar Cita";
        return View(vm);
    }

    // POST: /Citas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
    public async Task<IActionResult> Create(CitaCreateViewModel model)
    {
        if (!string.IsNullOrEmpty(model.HoraInicio) && !string.IsNullOrEmpty(model.HoraFin) &&
            string.CompareOrdinal(model.HoraFin, model.HoraInicio) <= 0)
        {
            ModelState.AddModelError("HoraFin", "La hora de fin debe ser posterior a la hora de inicio.");
        }

        if (!ModelState.IsValid)
        {
            await RecargarListasCitaAsync(model);
            return View(model);
        }

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dto = new CreateCitaDto
        {
            IdSolicitud = model.IdSolicitud,
            IdPsicologo = model.IdPsicologo,
            FechaCita = model.FechaCita.ToString("yyyy-MM-dd"),
            HoraInicio = FormatearHora(model.HoraInicio),
            HoraFin = FormatearHora(model.HoraFin),
            MinutosTolerancia = model.MinutosTolerancia
        };

        var result = await _citaService.CreateAsync(dto, token);
        if (result.Success)
        {
            TempData["Success"] = "Cita agendada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo agendar la cita.";
        await RecargarListasCitaAsync(model);
        return View(model);
    }

    // GET: /Citas/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Cita no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new CitaDetalleViewModel { Cita = result.Data };
        ViewBag.PageTitle = "Detalle de Cita";
        return View(vm);
    }

    // POST: /Citas/Confirmar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.ConfirmarAsync(id, token);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Cita confirmada exitosamente." : (result.Message ?? "No se pudo confirmar la cita.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Citas/Cancelar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.CancelarAsync(id, token);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Cita cancelada." : (result.Message ?? "No se pudo cancelar la cita.");
        return RedirectToAction(nameof(Index));
    }

    // POST: /Citas/Reagendar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reagendar(string id, CitaDetalleViewModel model)
    {
        if (!model.NuevaFecha.HasValue ||
            string.IsNullOrEmpty(model.NuevaHoraInicio) ||
            string.IsNullOrEmpty(model.NuevaHoraFin))
        {
            TempData["Error"] = "Debe especificar la nueva fecha y horario.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dto = new ReagendarCitaDto
        {
            NuevaFecha = model.NuevaFecha.Value,
            NuevaHoraInicio = model.NuevaHoraInicio!,
            NuevaHoraFin = model.NuevaHoraFin!,
            MotivoReagenda = model.MotivoReagenda
        };

        var result = await _citaService.ReagendarAsync(id, dto, token);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Cita reagendada exitosamente." : (result.Message ?? "No se pudo reagendar la cita.");
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Carga el ViewModel de creación de cita con solicitudes pendientes y psicólogos</summary>
    private async Task<CitaCreateViewModel> CargarDatosCitaAsync()
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;

        var solicitudesTask = _solicitudService.GetAllAsync(token);
        var usuariosTask = _usuarioService.GetAllAsync(token);
        await Task.WhenAll(solicitudesTask, usuariosTask);

        var solicitudes = solicitudesTask.Result.Data
            ?.Where(s => string.Equals(s.Estado, EstadosSolicitud.Pendiente, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList() ?? [];

        var psicologos = usuariosTask.Result.Data
            ?.Where(u => u.Rol == Roles.Psicologo)
            .ToList() ?? [];

        return new CitaCreateViewModel
        {
            SolicitudesPendientes = solicitudes,
            Psicologos = psicologos
        };
    }

    /// <summary>Recarga las listas del ViewModel cuando el POST falla</summary>
    private async Task RecargarListasCitaAsync(CitaCreateViewModel model)
    {
        var datos = await CargarDatosCitaAsync();
        model.SolicitudesPendientes = datos.SolicitudesPendientes;
        model.Psicologos = datos.Psicologos;
    }

    /// <summary>Normaliza una hora "HH:mm" a "HH:mm:ss"</summary>
    private static string FormatearHora(string hora)
    {
        var h = hora?.Trim();
        if (string.IsNullOrEmpty(h)) return string.Empty;
        return h.Length == 5 && h.Contains(':') ? $"{h}:00" : h;
    }
}

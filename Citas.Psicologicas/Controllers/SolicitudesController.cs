using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Solicitudes;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de solicitudes de atención psicológica</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Estudiante)]
public class SolicitudesController : Controller
{
    private readonly ISolicitudService _solicitudService;
    private readonly ILogger<SolicitudesController> _logger;

    public SolicitudesController(ISolicitudService solicitudService, ILogger<SolicitudesController> logger)
    {
        _solicitudService = solicitudService;
        _logger = logger;
    }

    // GET: /Solicitudes/AsignarCita/{id}  (flujo de asignación de cita desde la solicitud)
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    public async Task<IActionResult> AsignarCita(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _solicitudService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Solicitud no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Create", "Citas", new { idSolicitud = id });
    }

    // GET: /Solicitudes/Bandeja  (cola de atención de la psicóloga)
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    public Task<IActionResult> Bandeja()
    {
        ViewBag.PageTitle = "Bandeja de Solicitudes";
        ViewBag.Breadcrumb = new[] { ("Solicitudes", "/Solicitudes/Bandeja") };
        return Index(estado: EstadosSolicitud.Pendiente, prioridad: null, busqueda: null);
    }

    // GET: /Solicitudes
    public async Task<IActionResult> Index(string? estado, string? prioridad, string? busqueda)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _solicitudService.GetAllAsync(token);

        var solicitudes = result.Data ?? [];

        // Filtrar solo las del estudiante si el rol es Estudiante
        var rol = SessionHelper.GetRol(HttpContext.Session);
        if (rol == Roles.Estudiante)
        {
            var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
            solicitudes = solicitudes.Where(s => string.Equals(s.IdEstudiante?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrEmpty(estado))
            solicitudes = solicitudes.Where(s => string.Equals(s.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(prioridad))
            solicitudes = solicitudes.Where(s => string.Equals(s.Prioridad, prioridad, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(busqueda))
            solicitudes = solicitudes.Where(s =>
                (s.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Comentario?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        var vm = new SolicitudIndexViewModel
        {
            Solicitudes = solicitudes,
            FiltroEstado = estado,
            FiltroPrioridad = prioridad,
            FiltroBusqueda = busqueda
        };

        ViewBag.PageTitle = "Solicitudes de Atención";
        return View(vm);
    }

    // GET: /Solicitudes/Create
    [AuthorizeRole(Roles.Estudiante)]
    public IActionResult Create()
    {
        var vm = new SolicitudCreateViewModel
        {
            IdEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty
        };
        ViewBag.PageTitle = "Nueva Solicitud";
        return View(vm);
    }

    // POST: /Solicitudes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Estudiante)]
    public async Task<IActionResult> Create(SolicitudCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dto = new CreateSolicitudDto
        {
            IdEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty,
            MotivoConsulta = model.Comentario ?? string.Empty
        };

        var result = await _solicitudService.CreateAsync(dto, token);
        if (result.Success)
        {
            TempData["Success"] = "Solicitud enviada exitosamente. Se le notificará cuando sea atendida.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo enviar la solicitud.";
        return View(model);
    }

    // GET: /Solicitudes/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _solicitudService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Solicitud no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        // Regla de acceso: el estudiante solo ve sus propias solicitudes.
        if (SessionHelper.GetRol(HttpContext.Session) == Roles.Estudiante &&
            !string.Equals(result.Data.IdEstudianteStr, SessionHelper.GetIdUsuario(HttpContext.Session), StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No tiene permisos para ver esta solicitud.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.PageTitle = "Detalle de Solicitud";
        return View(result.Data);
    }

    // POST: /Solicitudes/AsignarPrioridad
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    public async Task<IActionResult> AsignarPrioridad(AsignarPrioridadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos inválidos para asignar prioridad.";
            return RedirectToAction(nameof(Details), new { id = model.IdSolicitud });
        }

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dto = new UpdatePrioridadDto
        {
            Prioridad = model.Prioridad?.ToUpperInvariant() ?? string.Empty,
            Observaciones = model.Observaciones,
            Estado = model.Estado?.ToUpperInvariant()
        };

        var result = await _solicitudService.UpdatePrioridadAsync(model.IdSolicitud, dto, token);
        if (result.Success)
            TempData["Success"] = $"Prioridad {model.Prioridad} asignada correctamente.";
        else
            TempData["Error"] = result.Message ?? "No se pudo asignar la prioridad.";

        return RedirectToAction(nameof(Details), new { id = model.IdSolicitud });
    }
}

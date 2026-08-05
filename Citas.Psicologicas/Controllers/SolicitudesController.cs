using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Solicitudes;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de solicitudes de atención psicológica</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Estudiante)]
public class SolicitudesController : Controller
{
    private readonly ISolicitudService _solicitudService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<SolicitudesController> _logger;

    public SolicitudesController(
        ISolicitudService solicitudService,
        ILocalDataService localData,
        ILogger<SolicitudesController> logger)
    {
        _solicitudService = solicitudService;
        _localData = localData;
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
            solicitudes = solicitudes.Where(s =>
                !string.IsNullOrEmpty(idUsuario) && (
                    string.Equals(s.IdEstudianteStr, idUsuario, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.IdEstudiante?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)
                )).ToList();
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
        ViewBag.SolicitudesCalendario = null;

        // La psicóloga ve las solicitudes directas del calendario que le fueron asignadas.
        var rolSesion = SessionHelper.GetRol(HttpContext.Session);
        if (rolSesion == Roles.Psicologo || rolSesion == Roles.Administrador)
        {
            var pendientes = _localData.GetSolicitudesCalendarioPendientes();
            if (rolSesion == Roles.Psicologo)
            {
                var idPsicologo = SessionHelper.GetIdUsuario(HttpContext.Session);
                pendientes = pendientes
                    .Where(sc => string.Equals(sc.IdPsicologo, idPsicologo, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            ViewBag.SolicitudesCalendario = pendientes;
        }

        return View(vm);
    }

    // GET: /Solicitudes/Create  (parámetros opcionales desde el calendario de disponibilidad)
    [AuthorizeRole(Roles.Estudiante)]
    public IActionResult Create(string? idPsicologo, string? nombrePsicologo, string? fechaCita, string? horaInicio, string? horaFin)
    {
        var vm = new SolicitudCreateViewModel
        {
            IdEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty,
            IdPsicologo = idPsicologo ?? string.Empty,
            NombrePsicologo = nombrePsicologo ?? string.Empty,
            FechaCita = DateTime.TryParse(fechaCita, out var fc) ? fc : null,
            HoraInicio = horaInicio ?? string.Empty,
            HoraFin = horaFin ?? string.Empty
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
            IdEstudiante = int.TryParse(SessionHelper.GetIdUsuario(HttpContext.Session), out var idEst) ? idEst : 0,
            Origen = OrigenSolicitud.Autonomo,
            MotivoConsulta = model.Comentario ?? string.Empty
        };

        var result = await _solicitudService.CreateAsync(dto, token);
        if (result.Success)
        {
            // Solicitud directa del calendario: se dirige a la psicóloga del horario elegido.
            if (model.DesdeCalendario && model.FechaCita.HasValue)
            {
                _localData.AddSolicitudCalendario(new SolicitudCalendario
                {
                    IdSolicitud = result.Data?.Id ?? string.Empty,
                    IdEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty,
                    NombreEstudiante = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty,
                    IdPsicologo = model.IdPsicologo,
                    NombrePsicologo = model.NombrePsicologo,
                    FechaCita = model.FechaCita.Value,
                    HoraInicio = model.HoraInicio,
                    HoraFin = model.HoraFin
                });
            }

            TempData["Success"] = "Solicitud enviada exitosamente. Se le notificará cuando sea atendida.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo enviar la solicitud.";
        return View(model);
    }

    // POST: /Solicitudes/MarcarAtendida/{id}  (solicitud directa del calendario)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    public IActionResult MarcarAtendida(int id)
    {
        _localData.MarcarSolicitudCalendarioAtendida(id);
        TempData["Success"] = "Solicitud de calendario marcada como atendida.";
        return RedirectToAction(nameof(Index));
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
        var result = await _solicitudService.UpdatePrioridadAsync(model.IdSolicitud, token);
        if (result.Success)
            TempData["Success"] = $"Prioridad recalculada exitosamente por motor Triage: {result.Data?.PrioridadCalculada ?? "ALTA"}.";
        else
            TempData["Error"] = result.Message ?? "No se pudo recalcular la prioridad.";

        return RedirectToAction(nameof(Details), new { id = model.IdSolicitud });
    }
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Solicitudes;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de solicitudes de atención psicológica</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor, Roles.Estudiante)]
public class SolicitudesController : Controller
{
    private readonly ISolicitudService _solicitudService;
    private readonly ICitaService _citaService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<SolicitudesController> _logger;

    public SolicitudesController(
        ISolicitudService solicitudService,
        ICitaService citaService,
        ILocalDataService localData,
        ILogger<SolicitudesController> logger)
    {
        _solicitudService = solicitudService;
        _citaService = citaService;
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

    // GET: /Solicitudes
    public async Task<IActionResult> Index(string? estado, string? prioridad, string? busqueda)
    {
        var rol = SessionHelper.GetRol(HttpContext.Session);
        var esEncargada = SessionHelper.EsPsicologaEncargada(HttpContext.Session, _localData);

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _solicitudService.GetAllAsync(token);

        var solicitudes = result.Data ?? [];

        // La psicóloga regular no ve el listado general; solo sus solicitudes directas.
        bool mostrarListadoGeneral = !(rol == Roles.Psicologo && !esEncargada);
        if (!mostrarListadoGeneral)
            solicitudes = [];

        // Filtrar solo las del estudiante si el rol es Estudiante
        if (rol == Roles.Estudiante)
        {
            var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
            solicitudes = solicitudes.Where(s =>
                !string.IsNullOrEmpty(idUsuario) && (
                    string.Equals(s.IdEstudianteStr, idUsuario, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.IdEstudiante?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)
                )).ToList();
        }
        else if (rol == Roles.Tutor)
        {
            var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
            var idsSolicitudesCanalizadas = _localData.GetCanalizacionesSolicitudes()
                .Where(cs => string.Equals(cs.IdTutor, idUsuario, StringComparison.OrdinalIgnoreCase))
                .Select(cs => cs.IdSolicitud)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            solicitudes = solicitudes.Where(s => idsSolicitudesCanalizadas.Contains(s.Id)).ToList();
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
        ViewBag.MostrarListadoGeneral = mostrarListadoGeneral;

        // La psicóloga ve las solicitudes directas del calendario que le fueron asignadas.
        var rolSesion = SessionHelper.GetRol(HttpContext.Session);
        if (rolSesion == Roles.Psicologo || rolSesion == Roles.Administrador)
        {
            var pendientes = _localData.GetSolicitudesCalendarioPendientes();
            if (rolSesion == Roles.Psicologo && !SessionHelper.EsPsicologaEncargada(HttpContext.Session, _localData))
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
    public async Task<IActionResult> Create(string? idPsicologo, string? nombrePsicologo, string? fechaCita, string? horaInicio, string? horaFin)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var idEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty;

        if (await VerificarSolicitudUnicaAsync(token, idEstudiante))
        {
            TempData["Error"] = "Ya tiene una solicitud o cita en proceso. " +
                                "Solo podrá solicitar nuevamente cuando alguna de sus citas sea cancelada " +
                                "o no haya asistido a la sesión.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new SolicitudCreateViewModel
        {
            IdEstudiante = idEstudiante,
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
        var idEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty;

        // Regla de negocio: un alumno solo puede tener UNA solicitud activa.
        // Se cancelan/cierran los duplicados existentes y se bloquea la nueva
        // hasta que alguna de sus citas sea cancelada o no haya asistido.
        if (await VerificarSolicitudUnicaAsync(token, idEstudiante))
        {
            TempData["Error"] = "Ya tiene una solicitud o cita en proceso. " +
                                "Solo podrá solicitar nuevamente cuando alguna de sus citas sea cancelada " +
                                "o no haya asistido a la sesión.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new CreateSolicitudDto
        {
            IdEstudiante = int.TryParse(idEstudiante, out var idEst) ? idEst : 0,
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
                    IdEstudiante = idEstudiante,
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

    /// <summary>
    /// Regla de negocio de una sola solicitud activa por alumno.
    /// Cancela/cierra las solicitudes, citas y agendas duplicadas del estudiante
    /// (conservando la más reciente de cada tipo) y devuelve true si el alumno
    /// todavía tiene una solicitud o cita en proceso.
    /// </summary>
    private async Task<bool> VerificarSolicitudUnicaAsync(string token, string idEstudiante)
    {
        var solicitudes = (await _solicitudService.GetAllAsync(token)).Data ?? [];
        var citas = (await _citaService.GetAllAsync(token)).Data ?? [];

        var misSolicitudes = solicitudes
            .Where(s => string.Equals(s.IdEstudianteStr, idEstudiante, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var misCitas = citas
            .Where(c => string.Equals(c.IdEstudianteStr, idEstudiante, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Solicitudes pendientes (aún sin cita) ordenadas por fecha, la más reciente primero.
        var pendientes = misSolicitudes
            .Where(s => s.Estado == EstadosSolicitud.Pendiente)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList();

        // Citas activas (reservada o confirmada) ordenadas por fecha, la más reciente primero.
        var citasActivas = misCitas
            .Where(c => c.Estado is EstadosCita.Reservada or EstadosCita.Confirmada)
            .OrderByDescending(c => c.Fecha)
            .ToList();

        // Limpieza de duplicados: cerrar las solicitudes pendientes antiguas.
        foreach (var duplicada in pendientes.Skip(1))
        {
            await _solicitudService.UpdatePrioridadAsync(duplicada.Id, new UpdatePrioridadDto
            {
                Prioridad = duplicada.Prioridad,
                Estado = EstadosSolicitud.Cancelada
            }, token);
        }

        // Limpieza de duplicados: cancelar las citas activas antiguas.
        foreach (var duplicada in citasActivas.Skip(1))
        {
            await _citaService.CancelarAsync(duplicada.Id, token);
        }

        // Limpieza de agendas duplicadas del calendario (respaldo local).
        var agendas = _localData.GetSolicitudesCalendario()
            .Where(sc => string.Equals(sc.IdEstudiante, idEstudiante, StringComparison.OrdinalIgnoreCase) && !sc.Atendida)
            .OrderByDescending(sc => sc.FechaRegistro)
            .ToList();
        foreach (var duplicada in agendas.Skip(1))
        {
            _localData.MarcarSolicitudCalendarioAtendida(duplicada.Id);
        }

        // Si aún queda una solicitud pendiente o una cita activa, el alumno está en proceso.
        return pendientes.Any() || citasActivas.Any();
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
        var result = await _solicitudService.UpdatePrioridadAsync(model.IdSolicitud, new UpdatePrioridadDto
        {
            Prioridad = model.Prioridad,
            Observaciones = model.Observaciones,
            Estado = model.Estado
        }, token);
        if (result.Success)
            TempData["Success"] = $"Prioridad asignada exitosamente: {result.Data?.PrioridadCalculada ?? model.Prioridad}.";
        else
            TempData["Error"] = result.Message ?? "No se pudo asignar la prioridad.";

        return RedirectToAction(nameof(Details), new { id = model.IdSolicitud });
    }
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Canalizaciones;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Canalizaciones;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de canalizaciones por tutores</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor)]
public class CanalizacionesController : Controller
{
    private readonly ICanalizacionService _canalizacionService;
    private readonly ISolicitudService _solicitudService;
    private readonly IUsuarioService _usuarioService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<CanalizacionesController> _logger;

    public CanalizacionesController(
        ICanalizacionService canalizacionService,
        ISolicitudService solicitudService,
        IUsuarioService usuarioService,
        ILocalDataService localData,
        ILogger<CanalizacionesController> logger)
    {
        _canalizacionService = canalizacionService;
        _solicitudService = solicitudService;
        _usuarioService = usuarioService;
        _localData = localData;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        var rol = SessionHelper.GetRol(HttpContext.Session);

        // El tutor solo ve la vista de creación de canalizaciones.
        if (rol == Roles.Tutor)
            return RedirectToAction(nameof(Create));

        // Solo la Psicóloga Encargada y el Administrador visualizan el listado.
        if (rol == Roles.Psicologo && !SessionHelper.EsPsicologaEncargada(HttpContext.Session, _localData))
            return RedirectToAction("AccessDenied", "Error");

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _canalizacionService.GetAllAsync(token);

        var canalizaciones = result.Data ?? [];

        if (!string.IsNullOrEmpty(busqueda))
            canalizaciones = canalizaciones.Where(c =>
                (c.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Motivo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        if (!string.IsNullOrEmpty(estado))
            canalizaciones = canalizaciones.Where(c => string.Equals(c.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

        var vm = new CanalizacionIndexViewModel
        {
            Canalizaciones = canalizaciones,
            Vinculos = _localData.GetCanalizacionesSolicitudes(),
            FiltroBusqueda = busqueda,
            FiltroEstado = estado
        };

        ViewBag.PageTitle = "Canalizaciones";
        return View(vm);
    }

    [AuthorizeRole(Roles.Tutor)]
    public async Task<IActionResult> Create()
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var usuarios = await _usuarioService.GetAllAsync(token);
        var estudiantes = usuarios.Data?.Where(u => u.Rol == Roles.Estudiante).ToList() ?? [];

        var vm = new CanalizacionCreateViewModel
        {
            IdTutor = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty,
            Estudiantes = estudiantes
        };

        ViewBag.PageTitle = "Nueva Canalización";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Tutor)]
    public async Task<IActionResult> Create(CanalizacionCreateViewModel model)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;

        if (!ModelState.IsValid)
        {
            var usuarios = await _usuarioService.GetAllAsync(token);
            model.Estudiantes = usuarios.Data?.Where(u => u.Rol == Roles.Estudiante).ToList() ?? [];
            return View(model);
        }

        var dto = new CreateCanalizacionDto
        {
            IdEstudiante = int.TryParse(model.IdEstudiante, out var idEst) ? idEst : 0,
            IdTutor = int.TryParse(SessionHelper.GetIdUsuario(HttpContext.Session) ?? model.IdTutor, out var idTutor) ? idTutor : 0,
            MotivoCanalizacion = model.Motivo,
            Observaciones = model.Observaciones ?? ""
        };

        var result = await _canalizacionService.CreateAsync(dto, token);
        if (!result.Success)
        {
            TempData["Error"] = result.Message ?? "No se pudo registrar la canalización.";
            return View(model);
        }

        var idCanalizacion = result.Data?.Id ?? string.Empty;

        // La canalización del tutor genera automáticamente una solicitud de atención (origen TITULAR)
        // para el estudiante; así puede ser agendada desde la vista de solicitudes.
        var solicitud = await _solicitudService.CreateAsync(new CreateSolicitudDto
        {
            IdEstudiante = int.TryParse(model.IdEstudiante, out var idEst2) ? idEst2 : 0,
            Origen = OrigenSolicitud.Tutoria,
            MotivoConsulta = model.Motivo,
            Prioridad = Prioridades.Media,
            PuntuacionTriage = 0
        }, token);

        if (solicitud.Success && !string.IsNullOrEmpty(solicitud.Data?.Id))
        {
            _localData.AddCanalizacionSolicitud(new CanalizacionSolicitud
            {
                IdCanalizacion = idCanalizacion,
                IdSolicitud = solicitud.Data.Id,
                IdEstudiante = solicitud.Data.IdEstudianteStr,
                NombreEstudiante = solicitud.Data.NombreEstudiante ?? string.Empty,
                IdTutor = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty,
                NombreTutor = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty,
                Motivo = model.Motivo,
                FechaRegistro = DateTime.Now
            });

            TempData["Success"] = "Canalización registrada. Se generó la solicitud de atención para el estudiante.";
        }
        else
        {
            TempData["Warning"] = "Canalización registrada, pero no se pudo generar la solicitud de atención.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Canalizaciones/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var canalizaciones = (await _canalizacionService.GetAllAsync(token)).Data ?? [];
        var canalizacion = canalizaciones.FirstOrDefault(c => c.Id == id);

        if (canalizacion is null)
        {
            TempData["Error"] = "Canalización no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var vinculo = _localData.GetCanalizacionesSolicitudes().FirstOrDefault(v => v.IdCanalizacion == id);
        ViewBag.VinculoSolicitud = vinculo;
        ViewBag.PageTitle = "Detalle de Canalización";
        ViewBag.Breadcrumb = new[] { ("Canalizaciones", "/Canalizaciones"), ("Detalle", "") };

        return View(canalizacion);
    }
}

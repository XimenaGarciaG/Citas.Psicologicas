using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Canalizaciones;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Canalizaciones;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de canalizaciones por tutores</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor)]
public class CanalizacionesController : Controller
{
    private readonly ICanalizacionService _canalizacionService;
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<CanalizacionesController> _logger;

    public CanalizacionesController(
        ICanalizacionService canalizacionService,
        IUsuarioService usuarioService,
        ILogger<CanalizacionesController> logger)
    {
        _canalizacionService = canalizacionService;
        _usuarioService = usuarioService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _canalizacionService.GetAllAsync(token);

        var canalizaciones = result.Data ?? [];

        var rol = SessionHelper.GetRol(HttpContext.Session);
        if (rol == Roles.Tutor)
        {
            var idTutor = SessionHelper.GetIdUsuario(HttpContext.Session);
            canalizaciones = canalizaciones.Where(c => string.Equals(c.IdTutor?.ToString(), idTutor, StringComparison.OrdinalIgnoreCase)).ToList();
        }

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
            FiltroBusqueda = busqueda,
            FiltroEstado = estado
        };

        ViewBag.PageTitle = "Canalizaciones";
        return View(vm);
    }

    [AuthorizeRole(Roles.Tutor, Roles.Administrador)]
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
    [AuthorizeRole(Roles.Tutor, Roles.Administrador)]
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
        if (result.Success)
        {
            TempData["Success"] = "Canalización registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo registrar la canalización.";
        return View(model);
    }
}

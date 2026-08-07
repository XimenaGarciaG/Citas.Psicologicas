using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Bitacora;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Bitacora;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de bitácora de asistencia personal y administrativa</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
public class BitacoraController : Controller
{
    private readonly IBitacoraService _bitacoraService;
    private readonly ICitaService _citaService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<BitacoraController> _logger;

    public BitacoraController(
        IBitacoraService bitacoraService,
        ICitaService citaService,
        ILocalDataService localData,
        ILogger<BitacoraController> logger)
    {
        _bitacoraService = bitacoraService;
        _citaService = citaService;
        _localData = localData;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? busqueda, DateTime? fecha)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty;
        var nombreUsuario = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty;
        var esEncargada = SessionHelper.EsPsicologaEncargada(HttpContext.Session, _localData);

        var result = await _bitacoraService.GetAllAsync(token);
        var registros = result.Data ?? [];

        // Filtrado por Bitácora Personal: la psicóloga regular ve sus propias asistencias registradas
        if (rol == Roles.Psicologo && !esEncargada)
        {
            registros = registros.Where(r =>
                !string.IsNullOrEmpty(r.NombrePsicologo) && r.NombrePsicologo.Contains(nombreUsuario, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (!string.IsNullOrEmpty(busqueda))
            registros = registros.Where(r =>
                (r.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.NombrePsicologo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        if (fecha.HasValue)
            registros = registros.Where(r => r.FechaCita?.Date == fecha.Value.Date).ToList();

        // Registros pendientes enviados por la psicóloga y pendientes de confirmación por email
        var pendientes = _localData.GetBitacorasPendientes()
            .Where(b => !b.Confirmada)
            .OrderByDescending(b => b.FechaEnvio)
            .ToList();

        if (rol == Roles.Psicologo && !esEncargada)
        {
            pendientes = pendientes.Where(b =>
                string.Equals(b.IdPsicologo, idUsuario, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(b.NombrePsicologo) && b.NombrePsicologo.Contains(nombreUsuario, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        var vm = new BitacoraIndexViewModel
        {
            Registros = registros,
            Pendientes = pendientes,
            FiltroBusqueda = busqueda,
            FiltroFecha = fecha
        };

        ViewBag.PageTitle = "Bitácora Personal de Asistencia";
        return View(vm);
    }

    public IActionResult Create()
    {
        TempData["Info"] = "Las bitácoras se generan y envían directamente desde el detalle de cada cita finalizada.";
        return RedirectToAction("Index", "Citas");
    }

    // GET: /Bitacora/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var registros = await _bitacoraService.GetAllAsync(token);

        var registro = registros.Data?.FirstOrDefault(r => r.Id == id);
        if (registro is null)
        {
            TempData["Error"] = "Registro no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.PageTitle = "Detalle de Bitácora";

        // Confirmación electrónica del estudiante sincronizada (respaldo local)
        var confirmacion = _localData.GetConfirmacion(registro.IdCita?.ToString() ?? string.Empty);
        ViewBag.ConfirmacionEstudiante = confirmacion;

        return View(registro);
    }
}

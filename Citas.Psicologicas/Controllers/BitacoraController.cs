using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Bitacora;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Bitacora;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de bitácora de asistencia administrativa</summary>
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
        var result = await _bitacoraService.GetAllAsync(token);

        var registros = result.Data ?? [];

        if (!string.IsNullOrEmpty(busqueda))
            registros = registros.Where(r =>
                (r.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.NombrePsicologo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        if (fecha.HasValue)
            registros = registros.Where(r => r.FechaCita?.Date == fecha.Value.Date).ToList();

        // Registros enviados por la psicóloga y pendientes de confirmación del estudiante.
        var pendientes = _localData.GetBitacorasPendientes()
            .Where(b => !b.Confirmada)
            .OrderByDescending(b => b.FechaEnvio)
            .ToList();

        var vm = new BitacoraIndexViewModel
        {
            Registros = registros,
            Pendientes = pendientes,
            FiltroBusqueda = busqueda,
            FiltroFecha = fecha
        };

        ViewBag.PageTitle = "Bitácora de Asistencia";
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var citasResult = await _citaService.GetAllAsync(token);
        var citasSinRegistro = citasResult.Data?
            .Where(c => c.Estado is EstadosCita.Confirmada or EstadosCita.Reservada)
            .OrderBy(c => c.Fecha)
            .ToList() ?? [];

        var vm = new BitacoraCreateViewModel { CitasSinRegistro = citasSinRegistro };
        ViewBag.PageTitle = "Registrar Asistencia";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BitacoraCreateViewModel model)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;

        if (!ModelState.IsValid)
        {
            var citasResult = await _citaService.GetAllAsync(token);
            model.CitasSinRegistro = citasResult.Data
                ?.Where(c => c.Estado is EstadosCita.Confirmada or EstadosCita.Reservada).ToList() ?? [];
            return View(model);
        }

        var citaResult = await _citaService.GetByIdAsync(model.IdCita, token);
        if (!citaResult.Success || citaResult.Data is null)
        {
            TempData["Error"] = "No se encontró la cita seleccionada.";
            return RedirectToAction(nameof(Index));
        }

        var cita = citaResult.Data;

        // Flujo de 2 fases: la psicóloga llena y envía el registro; el estudiante
        // lo confirma y solo entonces se registra en la API (bitácora).
        _localData.AddBitacoraPendiente(new BitacoraPendiente
        {
            IdCita = cita.Id,
            IdSolicitud = cita.IdSolicitud?.ToString() ?? string.Empty,
            IdEstudiante = cita.IdEstudianteStr,
            IdPsicologo = cita.IdPsicologoStr,
            NombreEstudiante = cita.NombreEstudiante ?? string.Empty,
            NombrePsicologo = cita.NombrePsicologo ?? string.Empty,
            Asistencia = model.Asistencia,
            Observaciones = model.Observaciones ?? string.Empty,
            AcuerdoSeguimiento = model.AcuerdoSeguimiento,
            FechaEnvio = DateTime.Now
        });

        _logger.LogInformation("Bitácora de la cita {Cita} enviada por {Psicologo}; pendiente de confirmación del estudiante",
            cita.Id, SessionHelper.GetNombreCompleto(HttpContext.Session));

        TempData["Success"] = "Registro enviado. El estudiante debe confirmarlo para que quede registrado en la bitácora.";
        return RedirectToAction(nameof(Index));
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

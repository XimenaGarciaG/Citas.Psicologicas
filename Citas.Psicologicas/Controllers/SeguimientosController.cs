using Microsoft.AspNetCore.Mvc;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Seguimientos;

namespace Citas.Psicologicas.Controllers;

public class SeguimientosController : Controller
{
    private readonly ILocalDataService _localData;
    private readonly ICitaService _citaService;
    private readonly ILogger<SeguimientosController> _logger;

    public SeguimientosController(ILocalDataService localData, ICitaService citaService, ILogger<SeguimientosController> logger)
    {
        _localData = localData;
        _citaService = citaService;
        _logger = logger;
    }

    // GET: /Seguimientos
    public async Task<IActionResult> Index(string? estado)
    {
        ViewBag.PageTitle = "Seguimientos";
        ViewBag.Breadcrumb = new[] { ("Seguimientos", "/Seguimientos") };

        var registros = _localData.GetSeguimientos()
            .OrderByDescending(s => s.FechaRegistro)
            .ToList();

        if (estado == "Pendientes")
        {
            registros = registros.Where(s => !s.Programado || s.FechaProgramada is null).ToList();
        }
        else if (estado == "Programados")
        {
            registros = registros.Where(s => s.Programado && s.FechaProgramada is not null).ToList();
        }

        // Enriquecer con la próxima cita real de cada estudiante (reservada o confirmada en el futuro).
        var proximas = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var token = SessionHelper.GetToken(HttpContext.Session);
            if (!string.IsNullOrEmpty(token))
            {
                var citas = (await _citaService.GetAllAsync(token)).Data ?? [];
                foreach (var cita in citas.Where(c =>
                             c.Estado is EstadosCita.Reservada or EstadosCita.Confirmada &&
                             c.Fecha >= DateTime.Today)
                             .OrderBy(c => c.Fecha))
                {
                    if (!string.IsNullOrEmpty(cita.IdEstudianteStr) &&
                        !proximas.ContainsKey(cita.IdEstudianteStr))
                    {
                        proximas[cita.IdEstudianteStr] = cita.Fecha;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron cargar las próximas citas para la vista de seguimientos.");
        }

        var vm = new SeguimientoIndexViewModel
        {
            Estado = estado,
            Seguimientos = registros.Select(s => new SeguimientoItem
            {
                Seguimiento = s,
                ProximaCita = !string.IsNullOrEmpty(s.IdEstudiante)
                    ? proximas.GetValueOrDefault(s.IdEstudiante)
                    : null
            }).ToList()
        };

        return View(vm);
    }

    // GET: /Seguimientos/Create
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    public IActionResult Create()
    {
        ViewBag.PageTitle = "Nuevo Seguimiento";
        return View(new SeguimientoRegistro { FechaProgramada = DateTime.Today.AddDays(7) });
    }

    // POST: /Seguimientos/Create
    [HttpPost]
    [AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SeguimientoRegistro model)
    {
        if (string.IsNullOrWhiteSpace(model.NombreEstudiante))
        {
            ModelState.AddModelError(nameof(model.NombreEstudiante), "El nombre del estudiante es obligatorio.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PageTitle = "Nuevo Seguimiento";
            return View(model);
        }

        model.Programado = model.FechaProgramada is not null && model.FechaProgramada > DateTime.Now;
        model.FechaRegistro = DateTime.Now;

        if (string.IsNullOrEmpty(model.IdPsicologo))
            model.IdPsicologo = Citas.Psicologicas.Helpers.SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty;
        if (string.IsNullOrEmpty(model.NombrePsicologo))
            model.NombrePsicologo = Citas.Psicologicas.Helpers.SessionHelper.GetNombreCompleto(HttpContext.Session) ?? "Psicóloga";

        _localData.AddSeguimiento(model);
        TempData["Success"] = "Seguimiento registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

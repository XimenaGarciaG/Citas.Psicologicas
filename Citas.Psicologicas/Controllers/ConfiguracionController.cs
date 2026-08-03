using Microsoft.AspNetCore.Mvc;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Controllers;

[AuthorizeRole(Roles.Administrador)]
public class ConfiguracionController : Controller
{
    private readonly ILocalDataService _localData;

    public ConfiguracionController(ILocalDataService localData)
    {
        _localData = localData;
    }

    // GET: /Configuracion
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Configuración";
        ViewBag.Breadcrumb = new[] { ("Configuración", "/Configuracion") };
        return View(_localData.GetConfiguracion());
    }

    // POST: /Configuracion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ConfiguracionSistema model)
    {
        if (!TimeSpan.TryParse(model.HorarioInicio, out var inicio) ||
            !TimeSpan.TryParse(model.HorarioFin, out var fin))
        {
            TempData["Error"] = "Los horarios deben tener formato HH:mm (ej. 08:00).";
            ViewBag.PageTitle = "Configuración";
            return View(model);
        }

        if (fin <= inicio)
        {
            ModelState.AddModelError(nameof(model.HorarioFin), "El horario de fin debe ser posterior al de inicio.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PageTitle = "Configuración";
            return View(model);
        }

        _localData.SaveConfiguracion(model);
        TempData["Success"] = "Configuración guardada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Mvc;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Controllers;

public class SeguimientosController : Controller
{
    private readonly ILocalDataService _localData;

    public SeguimientosController(ILocalDataService localData)
    {
        _localData = localData;
    }

    // GET: /Seguimientos
    public IActionResult Index(string? estado)
    {
        ViewBag.PageTitle = "Seguimientos";
        ViewBag.Breadcrumb = new[] { ("Seguimientos", "/Seguimientos") };

        var lista = _localData.GetSeguimientos()
            .OrderByDescending(s => s.FechaRegistro)
            .ToList();

        if (estado == "Pendientes")
        {
            lista = lista.Where(s => !s.Programado || s.FechaProgramada is null).ToList();
        }
        else if (estado == "Programados")
        {
            lista = lista.Where(s => s.Programado && s.FechaProgramada is not null).ToList();
        }

        ViewBag.Estado = estado;
        return View(lista);
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

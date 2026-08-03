using Microsoft.AspNetCore.Mvc;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Controllers;

[AuthorizeRole(Roles.Administrador)]
public class CatalogosController : Controller
{
    private readonly ILocalDataService _localData;

    public CatalogosController(ILocalDataService localData)
    {
        _localData = localData;
    }

    // GET: /Catalogos
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Catálogos";
        ViewBag.Breadcrumb = new[] { ("Catálogos", "/Catalogos") };
        ViewBag.Carreras = _localData.GetCarreras();
        ViewBag.Grupos = _localData.GetGrupos();
        return View();
    }

    // POST: /Catalogos/CrearCarrera
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearCarrera(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre de la carrera es obligatorio.";
            return RedirectToAction(nameof(Index));
        }

        var carreras = _localData.GetCarreras();
        carreras.Add(new Carrera { Id = carreras.Count == 0 ? 1 : carreras.Max(c => c.Id) + 1, Nombre = nombre.Trim() });
        _localData.SaveCarreras(carreras);
        TempData["Success"] = "Carrera agregada.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Catalogos/EditarCarrera/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarCarrera(int id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre de la carrera no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        var carreras = _localData.GetCarreras();
        var item = carreras.FirstOrDefault(c => c.Id == id);
        if (item is null)
        {
            TempData["Error"] = "Carrera no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        item.Nombre = nombre.Trim();
        _localData.SaveCarreras(carreras);
        TempData["Success"] = "Carrera actualizada.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Catalogos/EliminarCarrera/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarCarrera(int id)
    {
        var carreras = _localData.GetCarreras();
        carreras.RemoveAll(c => c.Id == id);
        _localData.SaveCarreras(carreras);
        TempData["Success"] = "Carrera eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Catalogos/CrearGrupo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearGrupo(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre del grupo es obligatorio.";
            return RedirectToAction(nameof(Index));
        }

        var grupos = _localData.GetGrupos();
        grupos.Add(new Grupo { Id = grupos.Count == 0 ? 1 : grupos.Max(g => g.Id) + 1, Nombre = nombre.Trim() });
        _localData.SaveGrupos(grupos);
        TempData["Success"] = "Grupo agregado.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Catalogos/EditarGrupo/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarGrupo(int id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre del grupo no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        var grupos = _localData.GetGrupos();
        var item = grupos.FirstOrDefault(g => g.Id == id);
        if (item is null)
        {
            TempData["Error"] = "Grupo no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        item.Nombre = nombre.Trim();
        _localData.SaveGrupos(grupos);
        TempData["Success"] = "Grupo actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Catalogos/EliminarGrupo/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarGrupo(int id)
    {
        var grupos = _localData.GetGrupos();
        grupos.RemoveAll(g => g.Id == id);
        _localData.SaveGrupos(grupos);
        TempData["Success"] = "Grupo eliminado.";
        return RedirectToAction(nameof(Index));
    }
}

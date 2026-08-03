using Microsoft.AspNetCore.Mvc;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.Controllers;

[AuthorizeRole(Roles.Psicologo, Roles.Administrador)]
public class NotificacionesController : Controller
{
    private readonly ILocalDataService _localData;

    public NotificacionesController(ILocalDataService localData)
    {
        _localData = localData;
    }

    // GET: /Notificaciones
    public IActionResult Index(string? tipo)
    {
        ViewBag.PageTitle = "Notificaciones";
        ViewBag.Breadcrumb = new[] { ("Notificaciones", "/Notificaciones") };

        var lista = _localData.GetNotificaciones()
            .OrderByDescending(n => n.Fecha)
            .ToList();

        if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
        {
            lista = lista.Where(n => string.Equals(n.Tipo, tipo, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        ViewBag.Tipos = TiposNotificacion;
        ViewBag.TipoSeleccionado = tipo;

        return View(lista);
    }

    // GET: /Notificaciones/MisNotificaciones  (notificaciones vinculadas al usuario actual)
    public IActionResult MisNotificaciones()
    {
        ViewBag.PageTitle = "Mis Notificaciones";
        ViewBag.Breadcrumb = new[] { ("Notificaciones", "/Notificaciones"), ("Mis Notificaciones", "/Notificaciones/MisNotificaciones") };

        var correoActual = SessionHelper.GetCorreo(HttpContext.Session);
        var nombreActual = SessionHelper.GetNombreCompleto(HttpContext.Session);

        var lista = _localData.GetNotificaciones()
            .Where(n =>
                string.Equals(n.CorreoDestinatario, correoActual, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.EnviadoPor, nombreActual, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.Fecha)
            .ToList();

        ViewBag.Tipos = TiposNotificacion;
        return View("Index", lista);
    }

    // GET: /Notificaciones/Enviar  (componer y registrar una notificación/correo)
    public IActionResult Enviar()
    {
        ViewBag.PageTitle = "Enviar Notificación";
        ViewBag.Breadcrumb = new[] { ("Notificaciones", "/Notificaciones"), ("Enviar", "/Notificaciones/Enviar") };
        return View(new NotificacionRegistro { Fecha = DateTime.Now });
    }

    // POST: /Notificaciones/Enviar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Enviar(NotificacionRegistro model)
    {
        if (string.IsNullOrWhiteSpace(model.CorreoDestinatario))
        {
            ModelState.AddModelError(nameof(model.CorreoDestinatario), "El correo del destinatario es obligatorio.");
        }
        if (string.IsNullOrWhiteSpace(model.Asunto))
        {
            ModelState.AddModelError(nameof(model.Asunto), "El asunto es obligatorio.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PageTitle = "Enviar Notificación";
            return View(model);
        }

        model.Fecha = DateTime.Now;
        model.EnviadoPor = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty;
        _localData.AddNotificacion(model);

        TempData["Success"] = "Notificación enviada y registrada en el historial.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Notificaciones/MarcarEnviado/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarcarEnviado(int id)
    {
        var lista = _localData.GetNotificaciones();
        var item = lista.FirstOrDefault(n => n.Id == id);
        if (item is not null)
        {
            item.Fecha = DateTime.Now;
            TempData["Success"] = "Notificación marcada como enviada.";
        }
        else
        {
            TempData["Error"] = "Notificación no encontrada.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static readonly string[] TiposNotificacion =
        ["Confirmacion", "Recordatorio", "Reagenda", "Cancelacion", "ResetPassword"];
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador principal – Dashboard del sistema</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor, Roles.Estudiante)]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IDashboardService dashboardService, ILogger<HomeController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _dashboardService.GetDashboardAsync(token);

        var vm = new DashboardViewModel
        {
            NombreUsuario = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? "",
            Rol = SessionHelper.GetRol(HttpContext.Session) ?? ""
        };

        if (result.Success && result.Data is not null)
            vm.Estadisticas = result.Data;
        else
            _logger.LogWarning("No se pudo cargar el dashboard: {Msg}", result.Message);

        ViewBag.PageTitle = "Dashboard";
        ViewBag.Breadcrumb = new[] { ("Inicio", "/") };
        return View(vm);
    }

    /// <summary>
    /// Renueva la sesión inactiva mientras el usuario mantiene la pestaña abierta
    /// (el write en sesión dispara el resellado de la cookie con nuevo IdleTimeout).
    /// </summary>
    [HttpGet]
    public IActionResult KeepAlive()
    {
        if (HttpContext.Session.IsAvailable)
            HttpContext.Session.SetString("LastKeepAlive", DateTime.UtcNow.Ticks.ToString());

        return Ok();
    }
}

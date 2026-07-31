using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador principal – Dashboard del sistema</summary>
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
}

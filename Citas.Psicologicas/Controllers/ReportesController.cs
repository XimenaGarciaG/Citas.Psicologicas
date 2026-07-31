using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de reportes administrativos con exportación a Excel</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
public class ReportesController : Controller
{
    private readonly IReporteService _reporteService;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IReporteService reporteService, ILogger<ReportesController> logger)
    {
        _reporteService = reporteService;
        _logger = logger;
    }

    // GET: /Reportes
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Reportes";
        ViewBag.BusquedaRealizada = false;
        return View(new ReporteFiltroViewModel());
    }

    // POST: /Reportes (consulta)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReporteFiltroViewModel model)
    {
        if (model.FechaFin < model.FechaInicio)
            ModelState.AddModelError("FechaFin", "La fecha fin no puede ser anterior a la fecha de inicio.");

        if (!ModelState.IsValid)
            return View(model);

        var token = SessionHelper.GetToken(HttpContext.Session)!;

        if (model.TipoReporte == "Asistencia")
        {
            var result = await _reporteService.GetAsistenciaAsync(model.FechaInicio, model.FechaFin, token);
            model.DatosAsistencia = result.Data ?? [];
        }
        else
        {
            var result = await _reporteService.GetInasistenciaAsync(model.FechaInicio, model.FechaFin, token);
            model.DatosInasistencia = result.Data ?? [];
        }

        ViewBag.PageTitle = "Reportes";
        ViewBag.BusquedaRealizada = true;
        return View(model);
    }

    // POST: /Reportes/ExportarExcel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarExcel(ReporteFiltroViewModel model)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;

        byte[] bytes;
        string fileName;

        if (model.TipoReporte == "Asistencia")
        {
            var result = await _reporteService.GetAsistenciaAsync(model.FechaInicio, model.FechaFin, token);
            bytes = await _reporteService.ExportarExcelAsistenciaAsync(result.Data ?? []);
            fileName = $"Reporte_Asistencia_{model.FechaInicio:yyyyMMdd}_{model.FechaFin:yyyyMMdd}.xlsx";
        }
        else
        {
            var result = await _reporteService.GetInasistenciaAsync(model.FechaInicio, model.FechaFin, token);
            bytes = await _reporteService.ExportarExcelInasistenciaAsync(result.Data ?? []);
            fileName = $"Reporte_Inasistencia_{model.FechaInicio:yyyyMMdd}_{model.FechaFin:yyyyMMdd}.xlsx";
        }

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

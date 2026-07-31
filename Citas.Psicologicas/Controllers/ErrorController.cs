using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de errores y manejo de códigos de estado HTTP</summary>
[Route("Error")]
public class ErrorController : Controller
{
    /// <summary>Punto de entrada para códigos de estado (UseStatusCodePagesWithReExecute)</summary>
    [HttpGet("{statusCode:int?}")]
    public IActionResult Index(int? statusCode)
    {
        ViewBag.StatusCode = statusCode ?? 500;
        return statusCode switch
        {
            401 => View("Error401"),
            404 => View("Error404"),
            _ => View("Error500")
        };
    }

    [HttpGet("Error401")]
    public IActionResult Error401()
    {
        ViewBag.StatusCode = 401;
        return View();
    }

    [HttpGet("Error404")]
    public IActionResult Error404()
    {
        ViewBag.StatusCode = 404;
        return View();
    }

    [HttpGet("Error500")]
    public IActionResult Error500()
    {
        ViewBag.StatusCode = 500;
        return View();
    }

    /// <summary>Maneja excepciones no controladas (UseExceptionHandler)</summary>
    [HttpGet("ServerError")]
    public IActionResult ServerError()
    {
        ViewBag.StatusCode = 500;
        return View("Error500");
    }

    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied() => View();
}

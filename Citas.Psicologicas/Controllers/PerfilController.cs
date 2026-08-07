using Citas.Psicologicas.Constants;
using Citas.Psicologicas.Extensions;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Perfil;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de perfil del usuario autenticado</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor, Roles.Estudiante)]
public class PerfilController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(
        IUsuarioService usuarioService,
        ILocalDataService localData,
        ILogger<PerfilController> logger)
    {
        _usuarioService = usuarioService;
        _localData = localData;
        _logger = logger;
    }

    // GET: /Perfil
    public async Task<IActionResult> Index()
    {
        var token = SessionHelper.GetToken(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
        var vm = new PerfilViewModel
        {
            Id = idUsuario ?? string.Empty,
            NombreCompleto = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty,
            Correo = SessionHelper.GetCorreo(HttpContext.Session) ?? string.Empty,
            Rol = SessionHelper.GetRol(HttpContext.Session) ?? string.Empty
        };

        if (!string.IsNullOrEmpty(idUsuario) && !string.IsNullOrEmpty(token))
        {
            var result = await _usuarioService.GetByIdAsync(idUsuario, token);
            if (result.Success && result.Data is not null)
            {
                var u = result.Data;
                vm.NombreCompleto = u.NombreCompleto;
                vm.Correo = u.Correo;
                vm.Rol = u.Rol;
                vm.Matricula = u.Matricula;
                vm.Carrera = u.Carrera;
                vm.Cuatrimestre = u.Cuatrimestre;
                vm.Grupo = u.Grupo;
                vm.EsRegular = u.EsRegular;
                vm.Departamento = u.Departamento;
                vm.CedulaProfesional = u.CedulaProfesional;
                vm.FechaCreacion = u.FechaCreacion;
                vm.Activo = u.Activo;
            }
        }

        ViewBag.PageTitle = "Mi Perfil";
        return View(vm);
    }

    // GET: /Perfil/CambiarContrasena
    public IActionResult CambiarContrasena()
    {
        ViewBag.PageTitle = "Cambiar Contraseña";
        return View(new CambiarContrasenaViewModel());
    }

    // POST: /Perfil/CambiarContrasena
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarContrasena(CambiarContrasenaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
        if (string.IsNullOrEmpty(idUsuario))
        {
            model.ErrorMessage = "No fue posible identificar su cuenta.";
            return View(model);
        }

        // Respaldo local: la API REST no expone un endpoint de cambio de contraseña.
        _localData.SetContrasenaLocal(idUsuario, model.NuevaContrasena);
        _logger.LogInformation("Contraseña actualizada localmente para el usuario {Id}", idUsuario);

        model.SuccessMessage = "Contraseña actualizada correctamente.";
        ModelState.Clear();
        return View(new CambiarContrasenaViewModel { SuccessMessage = model.SuccessMessage });
    }
}

using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Auth;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de autenticación: Login, Registro y Logout</summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (SessionHelper.IsAuthenticated(HttpContext.Session))
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new LoginRequestDto
        {
            Correo = model.Correo,
            Password = model.Contrasena
        };

        var result = await _authService.LoginAsync(dto);

        if (!result.Success || result.Data is null)
        {
            model.ErrorMessage = result.Message ?? "Credenciales incorrectas. Verifique su correo y contraseña.";
            _logger.LogWarning("Intento de login fallido para: {Correo}", model.Correo);
            return View(model);
        }

        var nombreMostrar = string.IsNullOrWhiteSpace(result.Data.NombreCompleto) 
            ? model.Correo.Split('@')[0] 
            : result.Data.NombreCompleto;

        SessionHelper.SetSession(
            HttpContext.Session,
            result.Data.Token,
            result.Data.Rol,
            result.Data.Correo,
            result.Data.GetIdUsuarioString(),
            nombreMostrar);

        _logger.LogInformation("Usuario {Correo} autenticado con rol {Rol}", result.Data.Correo, result.Data.Rol);
        TempData["Success"] = $"Bienvenido/a, {nombreMostrar}";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (SessionHelper.IsAuthenticated(HttpContext.Session))
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // El registro público solo permite roles de Estudiante o Tutor
        if (model.Rol is not (Roles.Estudiante or Roles.Tutor))
        {
            model.ErrorMessage = "El tipo de cuenta seleccionado no está permitido para el registro público.";
            return View(model);
        }

        var dto = new CreateUsuarioDto
        {
            Correo = model.Correo,
            Password = model.Password,
            Rol = model.Rol,
            NombreCompleto = model.NombreCompleto,
            Matricula = model.Matricula,
            Carrera = model.Carrera,
            Cuatrimestre = model.Cuatrimestre,
            Grupo = model.Grupo,
            EsRegular = model.EsRegular,
            Departamento = model.Departamento,
            CedulaProfesional = model.CedulaProfesional
        };

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            model.ErrorMessage = result.Message ?? "No se pudo crear la cuenta. Intente nuevamente.";
            return View(model);
        }

        TempData["Success"] = "¡Cuenta creada exitosamente! Ahora puede iniciar sesión con sus credenciales.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var token = SessionHelper.GetToken(HttpContext.Session);
        var nombre = SessionHelper.GetNombreCompleto(HttpContext.Session);

        if (!string.IsNullOrEmpty(token))
            await _authService.LogoutAsync(token);

        SessionHelper.ClearSession(HttpContext.Session);
        _logger.LogInformation("Usuario {Nombre} cerró sesión", nombre);

        return RedirectToAction("Login");
    }
}

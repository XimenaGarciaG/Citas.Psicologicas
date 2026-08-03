using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Auth;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de autenticación: Login, Registro, Recuperación y Logout</summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILocalDataService localData,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _localData = localData;
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

        return View(CargarRegistro(new RegisterViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(CargarRegistro(model));

        // El registro público solo permite crear cuentas de estudiantes.
        // Las psicólogas, tutores y administradores son creados por el Administrador.
        if (model.Rol != Roles.Estudiante)
        {
            model.ErrorMessage = "El registro público únicamente permite crear cuentas de estudiante.";
            return View(CargarRegistro(model));
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
            return View(CargarRegistro(model));
        }

        TempData["Success"] = "¡Cuenta creada exitosamente! Ahora puede iniciar sesión con sus credenciales.";
        return RedirectToAction("Login");
    }

    // GET: /Auth/ForgotPassword
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (SessionHelper.IsAuthenticated(HttpContext.Session))
            return RedirectToAction("Index", "Home");

        return View(new ForgotPasswordViewModel());
    }

    // POST: /Auth/ForgotPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Genera el token y lo guarda en el respaldo local (sin correo real configurado).
        var token = Guid.NewGuid().ToString("N");
        _localData.SetResetToken(model.Correo, token);
        _logger.LogInformation("Token de recuperación generado para: {Correo}", model.Correo);

        // Respuesta genérica de seguridad; el enlace solo se muestra en modo respaldo local
        // para que el flujo funcione sin un servicio de correo configurado.
        model.ResetLink = Url.Action("ResetPassword", "Auth", new { correo = model.Correo, token });
        return View(model);
    }

    // GET: /Auth/ResetPassword?correo=...&token=...
    [HttpGet]
    public IActionResult ResetPassword(string? correo, string? token)
    {
        if (SessionHelper.IsAuthenticated(HttpContext.Session))
            return RedirectToAction("Index", "Home");

        return View(new ResetPasswordViewModel
        {
            Correo = correo ?? string.Empty,
            Token = token ?? string.Empty
        });
    }

    // POST: /Auth/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var storedToken = _localData.GetResetToken(model.Correo);
        if (string.IsNullOrEmpty(storedToken) || !string.Equals(storedToken, model.Token, StringComparison.Ordinal))
        {
            model.ErrorMessage = "El enlace de recuperación no es válido o ya fue utilizado.";
            return View(model);
        }

        // La API hosteada no expone un endpoint para actualizar la contraseña,
        // por lo que el restablecimiento se persiste en el respaldo local.
        _localData.SetContrasenaLocalPorCorreo(model.Correo, model.Password);

        _localData.RemoveResetToken(model.Correo);
        _logger.LogInformation("Contraseña restablecida localmente para: {Correo}", model.Correo);
        TempData["Success"] = "Contraseña restablecida exitosamente. Ahora puede iniciar sesión.";
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

    /// <summary>Carga las carreras del catálogo en el ViewModel de registro</summary>
    private RegisterViewModel CargarRegistro(RegisterViewModel model)
    {
        model.CarrerasDisponibles = _localData.GetCarreras().Select(c => c.Nombre).ToList();
        return model;
    }
}

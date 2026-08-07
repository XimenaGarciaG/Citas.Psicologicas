using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Auth;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de autenticación: Login, Registro, Recuperación y Logout</summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly ILocalDataService _localData;
    private readonly INotificacionService _notificacionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUsuarioService usuarioService,
        ILocalDataService localData,
        INotificacionService notificacionService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _localData = localData;
        _notificacionService = notificacionService;
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
    [EnableRateLimiting("Auth")]
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

        // La API no siempre devuelve el nombre completo en el login; se consulta por ID.
        if (string.IsNullOrWhiteSpace(result.Data.NombreCompleto) &&
            !string.IsNullOrEmpty(result.Data.GetIdUsuarioString()))
        {
            var perfil = (await _usuarioService.GetByIdAsync(result.Data.GetIdUsuarioString(), result.Data.Token)).Data;
            if (perfil is not null && !string.IsNullOrWhiteSpace(perfil.NombreCompleto))
                nombreMostrar = perfil.NombreCompleto;
        }

        SessionHelper.SetSession(
            HttpContext.Session,
            result.Data.Token,
            result.Data.Rol,
            result.Data.Correo,
            result.Data.GetIdUsuarioString(),
            nombreMostrar);

        // Expiración absoluta: la sesión caduca como máximo a las 12 horas,
        // aunque el usuario esté activo (refuerza el cierre ante robo de sesión).
        HttpContext.Session.SetString(
            SessionKeys.ExpiraSesion,
            DateTime.Now.AddHours(12).ToString("o"));

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
    [EnableRateLimiting("Auth")]
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
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = Guid.NewGuid().ToString("N");
        _localData.SetResetToken(model.Correo, token);
        _logger.LogInformation("Token de recuperación generado para: {Correo}", model.Correo);

        var resetUrl = Url.Action("ResetPassword", "Auth", new { token }, Request.Scheme);
        model.ResetLink = resetUrl;

        var body = $@"
            <h2>Restablecimiento de Contraseña</h2>
            <p>Se ha solicitado restablecer la contraseña para la cuenta <strong>{model.Correo}</strong>.</p>
            <p>Haga clic en el siguiente enlace para restablecer su contraseña:</p>
            <p><a href='{resetUrl}'>{resetUrl}</a></p>
            <p>Si no solicitó este cambio, ignore este correo.</p>";

        var enviado = await _notificacionService.EnviarCorreoPersonalizadoAsync(model.Correo, "Restablecer contraseña - Citas Psicológicas", body);
        if (!enviado)
        {
            _logger.LogWarning(
                "No se pudo enviar el correo de recuperación a {Correo}; se muestra el enlace de respaldo.",
                model.Correo);
        }

        return View(model);
    }

    // GET: /Auth/ResetPassword?token=...
    [HttpGet]
    public IActionResult ResetPassword(string? token)
    {
        if (SessionHelper.IsAuthenticated(HttpContext.Session))
            return RedirectToAction("Index", "Home");

        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("ForgotPassword");

        // El correo se resuelve a partir del token; nunca viaja en la URL.
        var correo = _localData.GetEmailByResetToken(token);
        if (string.IsNullOrEmpty(correo))
        {
            return View(new ResetPasswordViewModel
            {
                ErrorMessage = "El enlace de recuperación no es válido o ya fue utilizado."
            });
        }

        return View(new ResetPasswordViewModel
        {
            Correo = correo,
            Token = token
        });
    }

    // POST: /Auth/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("Auth")]
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

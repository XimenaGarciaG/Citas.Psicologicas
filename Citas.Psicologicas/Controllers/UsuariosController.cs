using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Usuarios;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.ViewModels.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>CRUD de usuarios del sistema (solo Administrador)</summary>
[AuthorizeRole(Roles.Administrador)]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    // GET: /Usuarios
    public async Task<IActionResult> Index(string? busqueda, string? rol, string? estado)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _usuarioService.GetAllAsync(token);

        var usuarios = result.Data ?? [];

        if (!string.IsNullOrEmpty(busqueda))
            usuarios = usuarios.Where(u =>
                (u.NombreCompleto?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Correo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Matricula?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        if (!string.IsNullOrEmpty(rol))
            usuarios = usuarios.Where(u => string.Equals(u.Rol, rol, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(estado))
        {
            var activo = estado == "activo";
            usuarios = usuarios.Where(u => u.Activo == activo).ToList();
        }

        var vm = new UsuarioIndexViewModel
        {
            Usuarios = usuarios,
            FiltroBusqueda = busqueda,
            FiltroRol = rol,
            FiltroEstado = estado
        };

        ViewBag.PageTitle = "Gestión de Usuarios";
        return View(vm);
    }

    // GET: /Usuarios/Create
    public IActionResult Create()
    {
        ViewBag.PageTitle = "Nuevo Usuario";
        return View(new UsuarioCreateViewModel());
    }

    // POST: /Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = SessionHelper.GetToken(HttpContext.Session)!;
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

        var result = await _usuarioService.CreateAsync(dto, token);
        if (result.Success)
        {
            TempData["Success"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo crear el usuario.";
        return View(model);
    }

    // GET: /Usuarios/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _usuarioService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var u = result.Data;
        var vm = new UsuarioEditViewModel
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Correo = u.Correo,
            Rol = u.Rol,
            Matricula = u.Matricula,
            Carrera = u.Carrera,
            Cuatrimestre = u.Cuatrimestre,
            Grupo = u.Grupo,
            EsRegular = u.EsRegular ?? true,
            Departamento = u.Departamento,
            CedulaProfesional = u.CedulaProfesional,
            Activo = u.Activo
        };

        ViewBag.PageTitle = "Editar Usuario";
        return View(vm);
    }

    // POST: /Usuarios/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UsuarioEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dto = new UpdateUsuarioDto
        {
            Correo = model.Correo,
            NombreCompleto = model.NombreCompleto,
            Matricula = model.Matricula,
            Carrera = model.Carrera,
            Cuatrimestre = model.Cuatrimestre,
            Grupo = model.Grupo,
            EsRegular = model.EsRegular,
            Departamento = model.Departamento,
            CedulaProfesional = model.CedulaProfesional
        };

        var result = await _usuarioService.UpdateAsync(id, dto, token);
        if (result.Success)
        {
            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo actualizar el usuario.";
        return View(model);
    }

    // GET: /Usuarios/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _usuarioService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.PageTitle = "Detalle de Usuario";
        return View(result.Data);
    }
}

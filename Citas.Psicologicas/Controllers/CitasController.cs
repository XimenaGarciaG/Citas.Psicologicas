using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;
using Citas.Psicologicas.DTOs.Notificaciones;
using Citas.Psicologicas.DTOs.Solicitudes;
using Citas.Psicologicas.Filters;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;
using Citas.Psicologicas.Models;
using Citas.Psicologicas.ViewModels.Citas;
using Microsoft.AspNetCore.Mvc;

namespace Citas.Psicologicas.Controllers;

/// <summary>Controlador de agenda y gestión de citas psicológicas</summary>
[AuthorizeRole(Roles.Administrador, Roles.Psicologo, Roles.Tutor, Roles.Estudiante)]
public class CitasController : Controller
{
    private readonly ICitaService _citaService;
    private readonly ISolicitudService _solicitudService;
    private readonly IUsuarioService _usuarioService;
    private readonly INotificacionService _notificacionService;
    private readonly ILocalDataService _localData;
    private readonly ILogger<CitasController> _logger;

    public CitasController(
        ICitaService citaService,
        ISolicitudService solicitudService,
        IUsuarioService usuarioService,
        INotificacionService notificacionService,
        ILocalDataService localData,
        ILogger<CitasController> logger)
    {
        _citaService = citaService;
        _solicitudService = solicitudService;
        _usuarioService = usuarioService;
        _notificacionService = notificacionService;
        _localData = localData;
        _logger = logger;
    }

    // GET: /Citas
    public async Task<IActionResult> Index(string? estado, DateTime? inicio, DateTime? fin, string? busqueda, string vista = "Lista")
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.GetAllAsync(token);

        var citas = result.Data ?? [];

        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);

        // Filtrar según rol
        if (rol == Roles.Estudiante)
            citas = citas.Where(c => string.Equals(c.IdEstudiante?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();
        else if (rol == Roles.Psicologo)
            citas = citas.Where(c => string.Equals(c.IdPsicologo?.ToString(), idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(estado))
            citas = citas.Where(c => string.Equals(c.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

        if (inicio.HasValue)
            citas = citas.Where(c => c.Fecha >= inicio.Value).ToList();

        if (fin.HasValue)
            citas = citas.Where(c => c.Fecha <= fin.Value).ToList();

        if (!string.IsNullOrEmpty(busqueda))
            citas = citas.Where(c =>
                (c.NombreEstudiante?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.NombrePsicologo?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        var vm = new CitaIndexViewModel
        {
            Citas = citas,
            FiltroEstado = estado,
            FiltroFechaInicio = inicio,
            FiltroFechaFin = fin,
            FiltroBusqueda = busqueda,
            Vista = vista
        };

        ViewBag.PageTitle = "Agenda de Citas";
        ViewBag.CitasJson = System.Text.Json.JsonSerializer.Serialize(citas,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        return View("Index", vm);
    }

    // GET: /Citas/Agenda  (calendario día/semana/mes de la psicóloga)
    public async Task<IActionResult> Agenda(string? estado)
    {
        ViewBag.PageTitle = "Agenda";
        ViewBag.Breadcrumb = new[] { ("Agenda", "/Citas/Agenda") };
        return await Index(estado, null, null, null, "Calendario");
    }

    // GET: /Citas/Create  (idSolicitud: pre-selecciona la solicitud desde Bandeja/Detalle)
    [AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
    public async Task<IActionResult> Create(string? idSolicitud)
    {
        var vm = await CargarDatosCitaAsync();

        if (!string.IsNullOrEmpty(idSolicitud))
        {
            vm.IdSolicitud = idSolicitud;
            vm.SolicitudSeleccionada = (await _solicitudService.GetByIdAsync(idSolicitud, SessionHelper.GetToken(HttpContext.Session)!)).Data;

            // Si la solicitud fue hecha a una psicóloga concreta (desde el calendario de disponibilidad),
            // solo ella puede agendar esa cita.
            var solicitada = _localData.GetSolicitudesCalendario()
                .FirstOrDefault(s => string.Equals(s.IdSolicitud, idSolicitud, StringComparison.OrdinalIgnoreCase));
            if (solicitada is not null && !string.IsNullOrEmpty(solicitada.IdPsicologo))
            {
                vm.PsicologaSolicitadaId = solicitada.IdPsicologo;
                vm.PsicologaSolicitadaNombre = solicitada.NombrePsicologo;
                vm.IdPsicologo = solicitada.IdPsicologo;
            }

            // Regla de acceso: una psicóloga solo podrá asignar solicitudes dirigidas a ella;
            // las solicitudes presenciales solo las asigna la psicóloga encargada (administradora).
            if (!PuedeAsignarSolicitud(vm.SolicitudSeleccionada))
            {
                TempData["Error"] = "Solo la psicóloga encargada puede asignar solicitudes presenciales o de otras psicólogas. " +
                                    "Como psicóloga solo puede agendar las citas dirigidas específicamente a usted.";
                return RedirectToAction(nameof(Index));
            }
        }

        await CalcularDisponibilidadAsync(vm);
        ViewBag.PageTitle = "Agendar Cita";
        return View(vm);
    }

    // POST: /Citas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
    public async Task<IActionResult> Create(CitaCreateViewModel model)
    {
        if (!string.IsNullOrEmpty(model.HoraInicio) && !string.IsNullOrEmpty(model.HoraFin) &&
            string.CompareOrdinal(model.HoraFin, model.HoraInicio) <= 0)
        {
            ModelState.AddModelError("HoraFin", "La hora de fin debe ser posterior a la hora de inicio.");
        }

        if (!ModelState.IsValid)
        {
            await RecargarListasCitaAsync(model);
            return View(model);
        }

        var token = SessionHelper.GetToken(HttpContext.Session)!;

        var citasTask = _citaService.GetAllAsync(token);
        var usuariosTask = _usuarioService.GetAllAsync(token);
        var solicitudTask = _solicitudService.GetByIdAsync(model.IdSolicitud, token);
        await Task.WhenAll(citasTask, usuariosTask, solicitudTask);

        var citas = citasTask.Result.Data ?? [];
        var psicologos = usuariosTask.Result.Data?.Where(u => u.Rol == Roles.Psicologo).ToList() ?? [];
        var config = _localData.GetConfiguracion();
        var bloqueos = _localData.GetBloqueos(model.FechaCita);
        var solicitud = solicitudTask.Result.Data;

        // Regla de acceso: la psicóloga solo puede agendar solicitudes dirigidas a ella.
        if (!PuedeAsignarSolicitud(solicitud))
        {
            TempData["Error"] = "Solo la psicóloga encargada puede asignar solicitudes presenciales o de otras psicólogas. " +
                                "Como psicóloga solo puede agendar las citas dirigidas específicamente a usted.";
            return RedirectToAction(nameof(Index));
        }

        // Solicitud hecha a una psicóloga concreta desde el calendario → solo ella agenda.
        var solicitudCalendario = _localData.GetSolicitudesCalendario()
            .FirstOrDefault(s => string.Equals(s.IdSolicitud, model.IdSolicitud, StringComparison.OrdinalIgnoreCase));

        var idPsicologoElegida = solicitudCalendario is not null && !string.IsNullOrEmpty(solicitudCalendario.IdPsicologo)
            ? solicitudCalendario.IdPsicologo
            : model.IdPsicologo;

        // Una psicóloga regular únicamente agenda para sí misma; la encargada (Administrador) define la asignación.
        var rolUsuario = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
        var asignarSoloASiMisma = rolUsuario == Roles.Psicologo;
        if (asignarSoloASiMisma)
            psicologos = psicologos.Where(p => string.Equals(p.Id, idUsuario, StringComparison.OrdinalIgnoreCase)).ToList();

        // ── Asignación automática según disponibilidad ──────────────────────
        var (idPsicologoAsignada, horaInicio, horaFin, notaAsignacion) =
            ResolverAsignacion(model, citas, psicologos, bloqueos, config, idPsicologoElegida, asignarSoloASiMisma);

        if (string.IsNullOrEmpty(idPsicologoAsignada))
        {
            TempData["Error"] = "No hay psicólogas disponibles en esa fecha/horario. Intente con otra fecha u horario.";
            await RecargarListasCitaAsync(model);
            return View(model);
        }

        var dto = new CreateCitaDto
        {
            IdSolicitud = int.TryParse(model.IdSolicitud, out var idSol) ? idSol : 0,
            IdPsicologo = int.TryParse(idPsicologoAsignada, out var idPsi) ? idPsi : 0,
            FechaCita = model.FechaCita.ToString("yyyy-MM-dd"),
            HoraInicio = FormatearHora(horaInicio),
            HoraFin = FormatearHora(horaFin),
            MinutosTolerancia = model.MinutosTolerancia
        };

        var result = await _citaService.CreateAsync(dto, token);
        if (result.Success)
        {
            var psicologo = psicologos.FirstOrDefault(p => p.Id == idPsicologoAsignada);

            // Si era una solicitud específica del calendario, se marca como atendida (sincronización).
            if (solicitudCalendario is not null)
                _localData.MarcarSolicitudCalendarioAtendida(solicitudCalendario.Id);

            await NotificarCitaAgendadaAsync(solicitud, psicologo, model.FechaCita, horaInicio, notaAsignacion, token);

            var mensaje = string.IsNullOrEmpty(notaAsignacion)
                ? "Cita agendada exitosamente."
                : $"Cita agendada exitosamente. {notaAsignacion}";
            TempData["Success"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message ?? "No se pudo agendar la cita.";
        await RecargarListasCitaAsync(model);
        return View(model);
    }

    // GET: /Citas/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.GetByIdAsync(id, token);

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Cita no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
        var cita = result.Data;
        var config = _localData.GetConfiguracion();

        // Regla de acceso: el estudiante solo ve sus propias citas y el psicólogo las suyas.
        if ((rol == Roles.Estudiante && !string.Equals(cita.IdEstudianteStr, idUsuario, StringComparison.OrdinalIgnoreCase)) ||
            (rol == Roles.Psicologo && !string.Equals(cita.IdPsicologoStr, idUsuario, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["Error"] = "No tiene permisos para ver esta cita.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new CitaDetalleViewModel { Cita = cita };

        // Estado de la confirmación electrónica de asistencia (respaldo local)
        var confirmacion = _localData.GetConfirmacion(cita.Id);
        if (confirmacion is not null)
        {
            vm.AsistenciaConfirmada = confirmacion.Confirmada;
            vm.FechaConfirmacion = confirmacion.FechaConfirmacion;
        }

        // ¿El estudiante puede cancelar su propia cita? Solo dentro de la ventana configurada.
        if (rol == Roles.Estudiante && string.Equals(cita.IdEstudianteStr, idUsuario, StringComparison.OrdinalIgnoreCase))
        {
            vm.PuedeCancelarEstudiante =
                cita.Estado is EstadosCita.Reservada or EstadosCita.Confirmada &&
                cita.Fecha > DateTime.Now.AddHours(config.VentanaCancelacionHoras);

            vm.PuedeConfirmarAsistencia = EsConfirmable(cita, config) && !vm.AsistenciaConfirmada;
        }

        ViewBag.PageTitle = "Detalle de Cita";
        return View(vm);
    }

    // GET: /Citas/Disponibilidad  (estudiante: horarios libres y psicólogas disponibles)
    [AuthorizeRole(Roles.Estudiante)]
    public async Task<IActionResult> Disponibilidad(DateTime? fecha)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var dia = fecha ?? DateTime.Today;
        var config = _localData.GetConfiguracion();

        var citasTask = _citaService.GetAllAsync(token);
        var usuariosTask = _usuarioService.GetAllAsync(token);
        await Task.WhenAll(citasTask, usuariosTask);

        var citasDia = (citasTask.Result.Data ?? [])
            .Where(c => c.FechaCita?.Date == dia.Date)
            .ToList();

        var psicologas = usuariosTask.Result.Data
            ?.Where(u => u.Rol == Roles.Psicologo)
            .ToList() ?? [];

        var bloqueos = _localData.GetBloqueos(dia);
        var horarios = GenerarHorarios(config.HorarioInicio, config.HorarioFin, config.DuracionCitaMin);

        foreach (var h in horarios)
        {
            var inicio = TimeSpan.Parse(h.HoraInicio);
            var fin = TimeSpan.Parse(h.HoraFin);

            // Psicólogas con cita (no cancelada) en este horario → ocupadas.
            var citasSlot = citasDia.Where(c =>
                c.Estado != EstadosCita.Cancelada &&
                TimeSpan.TryParse(NormalizarHora(c.HoraInicio), out var hi) &&
                TimeSpan.TryParse(NormalizarHora(c.HoraFin), out var hf) &&
                hi < fin && hf > inicio).ToList();

            var ocupadas = new List<PsicologoDisponible>();
            foreach (var c in citasSlot)
            {
                ocupadas.Add(new PsicologoDisponible { Id = c.IdPsicologoStr, Nombre = c.NombrePsicologo ?? "Psicóloga" });
            }

            // Psicólogas con bloqueo que cubre este horario → no disponibles.
            foreach (var b in bloqueos.Where(b =>
                         TimeSpan.TryParse(b.HoraInicio, out var bi) && TimeSpan.TryParse(b.HoraFin, out var bf) &&
                         bi < fin && bf > inicio))
            {
                ocupadas.Add(new PsicologoDisponible { Id = b.IdPsicologo, Nombre = b.NombrePsicologo });
            }

            // Disponibles = psicólogas sin cita y sin bloqueo en el horario.
            var idsOcupadas = ocupadas.Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            h.PsicologosDisponibles = psicologas
                .Where(p => !idsOcupadas.Contains(p.Id))
                .Select(p => new PsicologoDisponible { Id = p.Id, Nombre = p.NombreCompleto })
                .ToList();

            h.PsicologosOcupados = ocupadas
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();
        }

        var vm = new DisponibilidadViewModel
        {
            Fecha = dia,
            FechaLabel = dia.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-MX")),
            Horarios = horarios
        };

        ViewBag.PageTitle = "Disponibilidad";
        return View(vm);
    }

    // GET: /Citas/MiDisponibilidad  (psicóloga: edita su calendario de atención)
    [AuthorizeRole(Roles.Psicologo)]
    public async Task<IActionResult> MiDisponibilidad(DateTime? fecha)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var idPsicologo = SessionHelper.GetIdUsuario(HttpContext.Session)!;
        var dia = fecha ?? DateTime.Today;
        var config = _localData.GetConfiguracion();

        var citasDia = (await _citaService.GetAllAsync(token)).Data?
            .Where(c => c.FechaCita?.Date == dia.Date &&
                        string.Equals(c.IdPsicologoStr, idPsicologo, StringComparison.OrdinalIgnoreCase) &&
                        c.Estado != EstadosCita.Cancelada)
            .ToList() ?? [];

        var bloqueos = _localData.GetBloqueos(dia)
            .Where(b => string.Equals(b.IdPsicologo, idPsicologo, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var horarios = GenerarHorarios(config.HorarioInicio, config.HorarioFin, config.DuracionCitaMin);
        foreach (var h in horarios)
        {
            var inicio = TimeSpan.Parse(h.HoraInicio);
            var fin = TimeSpan.Parse(h.HoraFin);

            var cita = citasDia.FirstOrDefault(c =>
                TimeSpan.TryParse(NormalizarHora(c.HoraInicio), out var hi) &&
                TimeSpan.TryParse(NormalizarHora(c.HoraFin), out var hf) &&
                hi < fin && hf > inicio);

            var bloqueo = bloqueos.FirstOrDefault(b =>
                TimeSpan.TryParse(b.HoraInicio, out var bi) &&
                TimeSpan.TryParse(b.HoraFin, out var bf) &&
                bi < fin && bf > inicio);

            h.EstadoOcupado = cita is not null ? "CITA" : (bloqueo is not null ? "BLOQUEADO" : "LIBRE");
            h.IdCita = cita?.Id;
        }

        var vm = new DisponibilidadViewModel
        {
            Fecha = dia,
            FechaLabel = dia.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-MX")),
            Horarios = horarios
        };

        ViewBag.PageTitle = "Mi Disponibilidad";
        ViewBag.Breadcrumb = new[] { ("Disponibilidad", "/Citas/MiDisponibilidad") };
        return View(vm);
    }

    // POST: /Citas/AlternarDisponibilidad  (bloquea/desbloquea un horario de la psicóloga)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Psicologo)]
    public IActionResult AlternarDisponibilidad(DateTime fecha, string horaInicio, string horaFin)
    {
        var idPsicologo = SessionHelper.GetIdUsuario(HttpContext.Session) ?? string.Empty;
        var nombrePsicologo = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? "Psicóloga";

        var bloqueo = _localData.GetBloqueos(fecha)
            .FirstOrDefault(b =>
                string.Equals(b.IdPsicologo, idPsicologo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(b.HoraInicio, horaInicio, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(b.HoraFin, horaFin, StringComparison.OrdinalIgnoreCase));

        if (bloqueo is not null)
        {
            _localData.RemoveBloqueo(bloqueo.Id);
            TempData["Success"] = $"Horario {horaInicio} – {horaFin} habilitado nuevamente.";
        }
        else
        {
            _localData.AddBloqueo(new BloqueoDisponibilidad
            {
                IdPsicologo = idPsicologo,
                NombrePsicologo = nombrePsicologo,
                Fecha = fecha,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Motivo = "No disponible"
            });
            TempData["Success"] = $"Horario {horaInicio} – {horaFin} marcado como no disponible.";
        }

        return RedirectToAction(nameof(MiDisponibilidad), new { fecha = fecha.ToString("yyyy-MM-dd") });
    }

    // POST: /Citas/ConfirmarAsistencia/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Estudiante)]
    public async Task<IActionResult> ConfirmarAsistencia(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var idEstudiante = SessionHelper.GetIdUsuario(HttpContext.Session);

        var result = await _citaService.GetByIdAsync(id, token);
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = "Cita no encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var cita = result.Data;
        if (!string.Equals(cita.IdEstudianteStr, idEstudiante, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No puede confirmar la asistencia de una cita que no le pertenece.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var config = _localData.GetConfiguracion();
        if (!EsConfirmable(cita, config))
        {
            TempData["Error"] = "No es posible confirmar la asistencia en este momento.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // La confirmación se sincroniza con la bitácora administrativa (respaldo local).
        _localData.SetConfirmacion(new ConfirmacionAsistencia
        {
            IdCita = cita.Id,
            IdEstudiante = idEstudiante ?? string.Empty,
            Confirmada = true,
            FechaConfirmacion = DateTime.Now
        });

        _logger.LogInformation("Estudiante {Id} confirmó asistencia de la cita {Cita}", idEstudiante, id);
        TempData["Success"] = "Asistencia confirmada. Su confirmación quedó sincronizada con la bitácora.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Citas/Confirmar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole(Roles.Administrador, Roles.Psicologo)]
    public async Task<IActionResult> Confirmar(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var result = await _citaService.ConfirmarAsync(id, token);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Cita confirmada exitosamente." : (result.Message ?? "No se pudo confirmar la cita.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Citas/Cancelar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(string id)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);

        // Regla de negocio: el estudiante solo puede cancelar sus propias citas dentro de la ventana.
        if (rol == Roles.Estudiante)
        {
            var result = await _citaService.GetByIdAsync(id, token);
            if (!result.Success || result.Data is null)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var cita = result.Data;
            var config = _localData.GetConfiguracion();
            if (!string.Equals(cita.IdEstudianteStr, idUsuario, StringComparison.OrdinalIgnoreCase) ||
                !(cita.Estado is EstadosCita.Reservada or EstadosCita.Confirmada) ||
                cita.Fecha <= DateTime.Now.AddHours(config.VentanaCancelacionHoras))
            {
                TempData["Error"] = "No es posible cancelar esta cita. Verifique las reglas de cancelación.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        var resultCancelar = await _citaService.CancelarAsync(id, token);
        if (resultCancelar.Success)
        {
            var citaCancelada = (await _citaService.GetByIdAsync(id, token)).Data;
            if (citaCancelada is not null)
            {
                RegistrarNotificacion(new Models.NotificacionRegistro
                {
                    Tipo = "Cancelacion",
                    IdEstudiante = citaCancelada.IdEstudianteStr,
                    NombreEstudiante = citaCancelada.NombreEstudiante ?? string.Empty,
                    Asunto = "Cita cancelada",
                    Cuerpo = $"Su cita del {citaCancelada.Fecha:dd/MM/yyyy} a las {citaCancelada.HoraInicio} fue cancelada.",
                    EnviadoPor = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty
                });
            }
            TempData["Success"] = "Cita cancelada.";
        }
        else
        {
            TempData["Error"] = resultCancelar.Message ?? "No se pudo cancelar la cita.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: /Citas/Reagendar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reagendar(string id, CitaDetalleViewModel model)
    {
        if (!model.NuevaFecha.HasValue ||
            string.IsNullOrEmpty(model.NuevaHoraInicio) ||
            string.IsNullOrEmpty(model.NuevaHoraFin))
        {
            TempData["Error"] = "Debe especificar la nueva fecha y horario.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var citaExistente = (await _citaService.GetByIdAsync(id, token)).Data;
        int.TryParse(citaExistente?.IdPsicologoStr, out var idPsicologo);

        var dto = new ReagendarCitaDto
        {
            IdPsicologo = idPsicologo > 0 ? idPsicologo : 1,
            FechaCita = model.NuevaFecha.Value.ToString("yyyy-MM-dd"),
            HoraInicio = FormatearHora(model.NuevaHoraInicio!),
            HoraFin = FormatearHora(model.NuevaHoraFin!),
            MinutosTolerancia = 15
        };

        var result = await _citaService.ReagendarAsync(id, dto, token);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Cita reagendada exitosamente." : (result.Message ?? "No se pudo reagendar la cita.");
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Carga el ViewModel de creación de cita con solicitudes pendientes y psicólogos</summary>
    private async Task<CitaCreateViewModel> CargarDatosCitaAsync()
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;

        var solicitudesTask = _solicitudService.GetAllAsync(token);
        var usuariosTask = _usuarioService.GetAllAsync(token);
        await Task.WhenAll(solicitudesTask, usuariosTask);

        var solicitudes = solicitudesTask.Result.Data
            ?.Where(s => string.Equals(s.Estado, EstadosSolicitud.Pendiente, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList() ?? [];

        var psicologos = usuariosTask.Result.Data
            ?.Where(u => u.Rol == Roles.Psicologo)
            .ToList() ?? [];

        var rol = SessionHelper.GetRol(HttpContext.Session);
        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);

        // Una psicóloga regular solo ve solicitudes dirigidas a ella y solo puede agendar para sí misma.
        if (rol == Roles.Psicologo)
        {
            var dirigidas = _localData.GetSolicitudesCalendario()
                .Where(s => string.Equals(s.IdPsicologo, idUsuario, StringComparison.OrdinalIgnoreCase) && !s.Atendida)
                .Select(s => s.IdSolicitud)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            solicitudes = solicitudes
                .Where(s => dirigidas.Contains(s.Id) || string.Equals(s.IdPsicologo, idUsuario, StringComparison.OrdinalIgnoreCase))
                .ToList();

            psicologos = psicologos
                .Where(p => string.Equals(p.Id, idUsuario, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new CitaCreateViewModel
        {
            SolicitudesPendientes = solicitudes,
            Psicologos = psicologos
        };
    }

    /// <summary>Recarga las listas del ViewModel cuando el POST falla</summary>
    private async Task RecargarListasCitaAsync(CitaCreateViewModel model)
    {
        var datos = await CargarDatosCitaAsync();
        model.SolicitudesPendientes = datos.SolicitudesPendientes;
        model.Psicologos = datos.Psicologos;
        await CalcularDisponibilidadAsync(model);
    }

    /// <summary>
    /// Regla de acceso de asignación de citas:
    /// — La psicóloga encargada (Administrador) es la única que puede asignar solicitudes
    ///   presenciales o dirigir a otras psicólogas.
    /// — Una psicóloga únicamente puede agendar la cita cuando la solicitud fue enviada
    ///   directamente a ella.
    /// </summary>
    private bool PuedeAsignarSolicitud(SolicitudDto? solicitud)
    {
        var rol = SessionHelper.GetRol(HttpContext.Session);
        if (rol == Roles.Administrador)
            return true;

        if (rol != Roles.Psicologo)
            return false;

        var idUsuario = SessionHelper.GetIdUsuario(HttpContext.Session);
        if (string.IsNullOrEmpty(idUsuario))
            return false;

        // La psicóloga solo agendará cuando la solicitud fue dirigida específicamente a ella.
        var idDirigida = solicitud?.IdPsicologo;
        if (string.IsNullOrEmpty(idDirigida))
        {
            var calendario = _localData.GetSolicitudesCalendario()
                .FirstOrDefault(s => string.Equals(s.IdSolicitud, solicitud?.Id, StringComparison.OrdinalIgnoreCase));
            idDirigida = calendario?.IdPsicologo;
        }

        return !string.IsNullOrEmpty(idDirigida) &&
               string.Equals(idDirigida, idUsuario, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Calcula la disponibilidad (horarios libres por psicóloga) y horarios de la fecha seleccionada</summary>
    private async Task CalcularDisponibilidadAsync(CitaCreateViewModel model)
    {
        var token = SessionHelper.GetToken(HttpContext.Session)!;
        var config = _localData.GetConfiguracion();
        var citas = (await _citaService.GetAllAsync(token)).Data ?? [];
        var bloqueos = _localData.GetBloqueos(model.FechaCita);

        var horarios = GenerarHorarios(config.HorarioInicio, config.HorarioFin, config.DuracionCitaMin);
        model.HorariosDisponibles = horarios.Select(h => $"{h.HoraInicio} – {h.HoraFin}").ToList();

        var mapa = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in model.Psicologos)
        {
            var libres = new List<string>();
            foreach (var h in horarios)
            {
                var inicio = TimeSpan.Parse(h.HoraInicio);
                var fin = TimeSpan.Parse(h.HoraFin);

                var ocupado = citas.Any(c =>
                    string.Equals(c.IdPsicologoStr, p.Id, StringComparison.OrdinalIgnoreCase) &&
                    c.FechaCita?.Date == model.FechaCita.Date &&
                    c.Estado != EstadosCita.Cancelada &&
                    TimeSpan.TryParse(NormalizarHora(c.HoraInicio), out var hi) &&
                    TimeSpan.TryParse(NormalizarHora(c.HoraFin), out var hf) &&
                    hi < fin && hf > inicio);

                var bloqueado = bloqueos.Any(b =>
                    string.Equals(b.IdPsicologo, p.Id, StringComparison.OrdinalIgnoreCase) &&
                    TimeSpan.TryParse(b.HoraInicio, out var bi) &&
                    TimeSpan.TryParse(b.HoraFin, out var bf) &&
                    bi < fin && bf > inicio);

                if (!ocupado && !bloqueado)
                    libres.Add(h.HoraInicio);
            }
            mapa[p.Id] = libres;
        }

        model.DisponibilidadPorPsicologa = mapa;
    }

    /// <summary>
    /// Resuelve automáticamente la psicóloga y el horario de la cita según la disponibilidad.
    /// Si la psicóloga elegida (o la solicitada) no tiene el horario, se asigna a otra disponible.
    /// </summary>
    private static (string IdPsicologo, string HoraInicio, string HoraFin, string? Nota) ResolverAsignacion(
        CitaCreateViewModel model,
        List<DTOs.Citas.CitaDto> citas,
        List<DTOs.Usuarios.UsuarioDto> psicologos,
        List<BloqueoDisponibilidad> bloqueos,
        ConfiguracionSistema config,
        string idPsicologoPreferida,
        bool soloASiMisma = false)
    {
        static bool Ocupado(string idPsi, string inicio, string fin, List<DTOs.Citas.CitaDto> citas,
            List<BloqueoDisponibilidad> bloqueos, DateTime fecha)
        {
            if (!TimeSpan.TryParse(inicio, out var ini) || !TimeSpan.TryParse(fin, out var finT))
                return true; // hora inválida → considerarla ocupada
            var enCita = citas.Any(c =>
                string.Equals(c.IdPsicologoStr, idPsi, StringComparison.OrdinalIgnoreCase) &&
                c.FechaCita?.Date == fecha.Date &&
                c.Estado != EstadosCita.Cancelada &&
                TimeSpan.TryParse(c.HoraInicio.Replace('"', ' ').Trim(), out var hi) &&
                TimeSpan.TryParse(c.HoraFin.Replace('"', ' ').Trim(), out var hf) &&
                hi < finT && hf > ini);
            var enBloqueo = bloqueos.Any(b =>
                string.Equals(b.IdPsicologo, idPsi, StringComparison.OrdinalIgnoreCase) &&
                TimeSpan.TryParse(b.HoraInicio, out var bi) &&
                TimeSpan.TryParse(b.HoraFin, out var bf) &&
                bi < finT && bf > ini);
            return enCita || enBloqueo;
        }

        // 1) Horario exacto solicitado por el usuario.
        var horario = new[] {
            (idPsicologoPreferida, model.HoraInicio, model.HoraFin),
            (model.IdPsicologo, model.HoraInicio, model.HoraFin)
        };
        foreach (var (idPsi, hIni, hFin) in horario)
        {
            if (string.IsNullOrEmpty(idPsi)) continue;
            if (!Ocupado(idPsi, hIni, hFin, citas, bloqueos, model.FechaCita.Date))
                return (idPsi, hIni, hFin, null);
        }

        // 2) Misma hora esbelta en otra psicóloga (cualquiera disponible en ese horario).
        //    (no aplica cuando la psicóloga regular solo agenda para sí misma)
        if (!soloASiMisma)
        {
            foreach (var p in psicologos)
            {
                if (!Ocupado(p.Id, model.HoraInicio, model.HoraFin, citas, bloqueos, model.FechaCita.Date))
                    return (p.Id, model.HoraInicio, model.HoraFin,
                        $"El horario {model.HoraInicio}–{model.HoraFin} no estaba disponible para la psicóloga seleccionada; se asignó a {p.NombreCompleto}.");
            }
        }

        // 3) Si la psicóloga preferida está ocupada todo el día, se busca un horario libre con ella.
        if (!string.IsNullOrEmpty(idPsicologoPreferida))
        {
            var horarios = GenerarHorarios(config.HorarioInicio, config.HorarioFin, config.DuracionCitaMin);
            foreach (var h in horarios)
            {
                if (!Ocupado(idPsicologoPreferida, h.HoraInicio, h.HoraFin, citas, bloqueos, model.FechaCita.Date))
                    return (idPsicologoPreferida, h.HoraInicio, h.HoraFin,
                        $"La psicóloga solicitada no tenía libre el horario elegido; se reagendó automáticamente a {h.HoraInicio}–{h.HoraFin}.");
            }
        }

        // 4) Cualquier psicóloga con cualquier horario libre.
        //    (no aplica cuando la psicóloga regular solo agenda para sí misma)
        if (!soloASiMisma)
        {
            foreach (var p in psicologos)
            {
                var horarios = GenerarHorarios(config.HorarioInicio, config.HorarioFin, config.DuracionCitaMin);
                foreach (var h in horarios)
                {
                    if (!Ocupado(p.Id, h.HoraInicio, h.HoraFin, citas, bloqueos, model.FechaCita.Date))
                        return (p.Id, h.HoraInicio, h.HoraFin,
                            $"No había disponibilidad; se asignó automáticamente con {p.NombreCompleto} a {h.HoraInicio}–{h.HoraFin}.");
                }
            }
        }

        return (string.Empty, string.Empty, string.Empty, null);
    }

    /// <summary>Notifica por correo al estudiante (vía API) y registra la notificación en el historial local</summary>
    private async Task NotificarCitaAgendadaAsync(
        SolicitudDto? solicitud,
        DTOs.Usuarios.UsuarioDto? psicologo,
        DateTime fecha,
        string horaInicio,
        string? notaAsignacion,
        string token)
    {
        var correoEstudiante = string.Empty;
        var nombreEstudiante = solicitud?.NombreEstudiante ?? string.Empty;

        if (solicitud is not null)
        {
            var estudiante = (await _usuarioService.GetByIdAsync(solicitud.IdEstudianteStr, token)).Data;
            correoEstudiante = estudiante?.Correo ?? string.Empty;
            if (string.IsNullOrEmpty(nombreEstudiante))
                nombreEstudiante = estudiante?.NombreCompleto ?? string.Empty;
        }

        var cuerpo = $"Su cita del {fecha:dd/MM/yyyy} a las {horaInicio} fue agendada con " +
                     $"{psicologo?.NombreCompleto ?? "el psicólogo/a asignado"}.";
        if (!string.IsNullOrEmpty(notaAsignacion))
            cuerpo = $"{notaAsignacion} Su cita quedó agendada el {fecha:dd/MM/yyyy} a las {horaInicio}.";

        RegistrarNotificacion(new Models.NotificacionRegistro
        {
            Tipo = "Confirmacion",
            IdEstudiante = solicitud?.IdEstudianteStr ?? string.Empty,
            CorreoDestinatario = correoEstudiante,
            NombreEstudiante = nombreEstudiante,
            Asunto = "Cita psicológica agendada",
            Cuerpo = cuerpo,
            EnviadoPor = SessionHelper.GetNombreCompleto(HttpContext.Session) ?? string.Empty
        });

        try
        {
            if (!string.IsNullOrEmpty(correoEstudiante))
            {
                var enviado = await _notificacionService.EnviarRecordatorioAsync(new NotificacionRequestDto
                {
                    EmailDestino = correoEstudiante,
                    NombrePaciente = nombreEstudiante,
                    FechaCita = fecha.ToString("yyyy-MM-dd"),
                    HoraCita = horaInicio.Length >= 5 ? horaInicio[..5] : horaInicio
                }, token);

                if (!enviado)
                    _logger.LogWarning("No se pudo enviar el correo de confirmación de cita a {Email}", correoEstudiante);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ocurrió un error al enviar la notificación por correo de la cita.");
        }
    }

    /// <summary>Normaliza una hora "HH:mm" a "HH:mm:ss"</summary>
    private static string FormatearHora(string hora)
    {
        var h = hora?.Trim();
        if (string.IsNullOrEmpty(h)) return string.Empty;
        return h.Length == 5 && h.Contains(':') ? $"{h}:00" : h;
    }

    /// <summary>Determina si la cita puede confirmarse electrónicamente por el estudiante</summary>
    private static bool EsConfirmable(DTOs.Citas.CitaDto cita, Models.ConfiguracionSistema config)
    {
        if (cita.Estado == EstadosCita.Cancelada || cita.Estado == EstadosCita.Reagendada)
            return false;

        if (cita.Estado == EstadosCita.Concluida)
            return true;

        // Si la hora de fin no es válida no es posible calcular la ventana de confirmación.
        if (!TimeSpan.TryParse(FormatearHora(cita.HoraFin), out var horaFin))
            return false;

        var fin = cita.Fecha.Date.Add(horaFin);
        var limite = fin.AddHours(config.VentanaConfirmacionHoras);
        return DateTime.Now >= fin && DateTime.Now <= limite;
    }

    /// <summary>Genera la rejilla de horarios según configuración</summary>
    private static List<HorarioDisponible> GenerarHorarios(string inicio, string fin, int duracionMin)
    {
        var horarios = new List<HorarioDisponible>();
        if (!TimeSpan.TryParse(inicio, out var horaInicio) ||
            !TimeSpan.TryParse(fin, out var horaFin) ||
            duracionMin <= 0)
        {
            return horarios;
        }

        for (var t = horaInicio; t < horaFin; t = t.Add(TimeSpan.FromMinutes(duracionMin)))
        {
            var end = t.Add(TimeSpan.FromMinutes(duracionMin));
            if (end > horaFin) break;
            horarios.Add(new HorarioDisponible
            {
                HoraInicio = t.ToString(@"hh\:mm"),
                HoraFin = end.ToString(@"hh\:mm")
            });
        }

        return horarios;
    }

    /// <summary>Normaliza una hora para comparar ("HH:mm" u "HH:mm:ss")</summary>
    private static string NormalizarHora(string hora)
    {
        if (string.IsNullOrWhiteSpace(hora)) return string.Empty;
        var h = hora.Trim();
        if (TimeSpan.TryParse(h, out var ts))
            return ts.ToString(@"hh\:mm");
        return h;
    }

    /// <summary>Registra una notificación en el historial local (respaldo de envío de correos)</summary>
    private void RegistrarNotificacion(Models.NotificacionRegistro notificacion)
    {
        try
        {
            notificacion.Fecha = DateTime.Now;
            _localData.AddNotificacion(notificacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar la notificación localmente.");
        }
    }
}

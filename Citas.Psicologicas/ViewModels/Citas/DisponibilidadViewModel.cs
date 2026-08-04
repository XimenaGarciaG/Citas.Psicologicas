namespace Citas.Psicologicas.ViewModels.Citas;

/// <summary>ViewModel para el calendario de disponibilidad del estudiante</summary>
public class DisponibilidadViewModel
{
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string FechaLabel { get; set; } = string.Empty;
    public List<HorarioDisponible> Horarios { get; set; } = [];
    public int Disponibles => Horarios.Count(h => h.PsicologosDisponibles.Any());
    public int Ocupados => Horarios.Count(h => !h.PsicologosDisponibles.Any());
}

/// <summary>Horario individual dentro del calendario de disponibilidad</summary>
public class HorarioDisponible
{
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;

    /// <summary>Psicólogas disponibles en este horario (sin cita ni bloqueo)</summary>
    public List<PsicologoDisponible> PsicologosDisponibles { get; set; } = [];

    /// <summary>Psicólogas ocupadas en este horario (con cita o bloqueadas)</summary>
    public List<PsicologoDisponible> PsicologosOcupados { get; set; } = [];

    public string? IdCita { get; set; }
    public string? EstadoOcupado { get; set; }
    public bool Ocupado => !PsicologosDisponibles.Any();
}

/// <summary>Psicóloga con su estado en un horario específico</summary>
public class PsicologoDisponible
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

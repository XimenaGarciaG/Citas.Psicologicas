namespace Citas.Psicologicas.ViewModels.Citas;

/// <summary>ViewModel para el calendario de disponibilidad del estudiante</summary>
public class DisponibilidadViewModel
{
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string FechaLabel { get; set; } = string.Empty;
    public List<HorarioDisponible> Horarios { get; set; } = [];
    public int Disponibles => Horarios.Count(h => !h.Ocupado);
    public int Ocupados => Horarios.Count(h => h.Ocupado);
}

/// <summary>Horario individual dentro del calendario de disponibilidad</summary>
public class HorarioDisponible
{
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public bool Ocupado { get; set; }
    public string? IdCita { get; set; }
    public string? EstadoOcupado { get; set; }
}

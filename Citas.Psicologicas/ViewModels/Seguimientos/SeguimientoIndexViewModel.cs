using Citas.Psicologicas.Models;

namespace Citas.Psicologicas.ViewModels.Seguimientos;

/// <summary>ViewModel del listado de seguimientos con la próxima cita del estudiante</summary>
public class SeguimientoIndexViewModel
{
    public List<SeguimientoItem> Seguimientos { get; set; } = [];
    public string? Estado { get; set; }
}

/// <summary>Ítem de seguimiento con la próxima cita programada del estudiante</summary>
public class SeguimientoItem
{
    public SeguimientoRegistro Seguimiento { get; set; } = new();

    /// <summary>Próxima cita real del estudiante (reservada o confirmada) en el futuro</summary>
    public DateTime? ProximaCita { get; set; }

    public string NombreEstudiante => Seguimiento.NombreEstudiante;
    public string NombrePsicologo => Seguimiento.NombrePsicologo ?? "—";
    public string? Motivo => Seguimiento.Motivo;
    public string? Notas => Seguimiento.Notas;
    public DateTime FechaRegistro => Seguimiento.FechaRegistro;
    public bool Programado => Seguimiento.Programado;
    public DateTime? FechaProgramada => Seguimiento.FechaProgramada;

    /// <summary>Fecha efectiva a mostrar: la próxima cita real si existe, si no la programada del seguimiento</summary>
    public DateTime? FechaMostrar => ProximaCita ?? FechaProgramada;
}

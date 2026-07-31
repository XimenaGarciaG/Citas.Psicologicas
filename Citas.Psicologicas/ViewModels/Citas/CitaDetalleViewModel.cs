using System.ComponentModel.DataAnnotations;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Citas;

namespace Citas.Psicologicas.ViewModels.Citas;

/// <summary>ViewModel para el detalle de una cita con opciones de reagenda</summary>
public class CitaDetalleViewModel
{
    public CitaDto Cita { get; set; } = new();

    [DataType(DataType.Date)]
    [Display(Name = "Nueva Fecha")]
    public DateTime? NuevaFecha { get; set; }

    [Display(Name = "Nueva Hora de Inicio")]
    public string? NuevaHoraInicio { get; set; }

    [Display(Name = "Nueva Hora de Fin")]
    public string? NuevaHoraFin { get; set; }

    [StringLength(500)]
    [Display(Name = "Motivo de Reagenda")]
    public string? MotivoReagenda { get; set; }

    public bool PuedeConfirmar => Cita.Estado == EstadosCita.Reservada;
    public bool PuedeCancelar => Cita.Estado is EstadosCita.Reservada or EstadosCita.Confirmada;
    public bool PuedeReagendar => Cita.Estado is EstadosCita.Reservada or EstadosCita.Confirmada;
}

using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Bitacora;

/// <summary>DTO de lectura para bitácora devuelto por GET /bitacora</summary>
public class BitacoraDto
{
    [JsonPropertyName("idBitacora")]
    public object? IdBitacora { get; set; }

    [JsonPropertyName("idCita")]
    public object? IdCita { get; set; }

    public string? NombreEstudiante { get; set; }
    public string? NombrePsicologo { get; set; }
    public bool Asistencia { get; set; }
    public string? Observaciones { get; set; }
    public bool AcuerdoSeguimiento { get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? FechaCita { get; set; }

    public string Id => IdBitacora?.ToString() ?? "";
    public bool Asistio => Asistencia;
    public string ObservacionesAdministrativas => Observaciones ?? "";
    public bool RequiereSeguimiento => AcuerdoSeguimiento;
    public string BadgeAsistencia => Asistencia ? "success" : "danger";
    public string TextoAsistencia => Asistencia ? "Asistió" : "No asistió";
}

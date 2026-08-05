using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Bitacora;

/// <summary>DTO de lectura para bitácora devuelto por GET /bitacora</summary>
public class BitacoraDto
{
    [JsonPropertyName("idBitacora")]
    public object? IdBitacora { get; set; }

    [JsonPropertyName("idCita")]
    public object? IdCita { get; set; }

    [JsonPropertyName("nombreEstudiante")]
    public string? NombreEstudiante { get; set; }

    [JsonPropertyName("nombrePsicologo")]
    public string? NombrePsicologo { get; set; }

    [JsonPropertyName("asistencia")]
    public bool Asistencia { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("acuerdoSeguimiento")]
    public bool AcuerdoSeguimiento { get; set; }

    [JsonPropertyName("fechaCierre")]
    public DateTime? Fecha { get; set; }

    [JsonPropertyName("fechaCita")]
    public DateTime? FechaCita { get; set; }

    public string Id => IdBitacora?.ToString() ?? "";
    public bool Asistio => Asistencia;
    public string ObservacionesAdministrativas => Observaciones ?? "";
    public bool RequiereSeguimiento => AcuerdoSeguimiento;
    public string BadgeAsistencia => Asistencia ? "success" : "danger";
    public string TextoAsistencia => Asistencia ? "Asistió" : "No asistió";
}
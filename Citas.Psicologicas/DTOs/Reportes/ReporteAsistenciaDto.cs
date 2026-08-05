using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Reportes;

public class ReporteRespuestaDto
{
    [JsonPropertyName("totalRegistros")]
    public int TotalRegistros { get; set; }

    [JsonPropertyName("porcentajeTotal")]
    public double PorcentajeTotal { get; set; }

    [JsonPropertyName("detalle")]
    public List<ReporteDetalleItemDto> Detalle { get; set; } = [];
}

public class ReporteDetalleItemDto
{
    [JsonPropertyName("idBitacora")]
    public object? IdBitacora { get; set; }

    [JsonPropertyName("idCita")]
    public object? IdCita { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime? Fecha { get; set; }

    [JsonPropertyName("observaciones")]
    public string? Observaciones { get; set; }

    [JsonPropertyName("nombreEstudiante")]
    public string? NombreEstudiante { get; set; }

    [JsonPropertyName("numeroControl")]
    public string? NumeroControl { get; set; }

    [JsonPropertyName("carrera")]
    public string? Carrera { get; set; }

    [JsonPropertyName("nombrePsicologo")]
    public string? NombrePsicologo { get; set; }

    [JsonPropertyName("horaInicio")]
    public string? HoraInicio { get; set; }

    [JsonPropertyName("asistio")]
    public bool Asistio { get; set; } = true;
}

public class ReporteAsistenciaDto : ReporteDetalleItemDto
{
    public string IdEstudiante => IdBitacora?.ToString() ?? "";
}

public class ReporteInasistenciaDto : ReporteDetalleItemDto
{
    public string IdEstudiante => IdBitacora?.ToString() ?? "";
    public DateTime FechaCita => Fecha ?? DateTime.Today;
    public string Motivo => Observaciones ?? "Inasistencia registrada";
}
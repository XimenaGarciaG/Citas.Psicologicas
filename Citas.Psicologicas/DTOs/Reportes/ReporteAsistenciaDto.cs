using System.Text.Json.Serialization;

namespace Citas.Psicologicas.DTOs.Reportes;

/// <summary>DTO wrapper para respuesta de reportes de la API REST</summary>
public class ReporteRespuestaDto
{
    public int TotalRegistros { get; set; }
    public double PorcentajeTotal { get; set; }
    public List<ReporteDetalleItemDto> Detalle { get; set; } = [];
}

/// <summary>Detalle de cada registro en el reporte</summary>
public class ReporteDetalleItemDto
{
    [JsonPropertyName("idBitacora")]
    public object? IdBitacora { get; set; }

    [JsonPropertyName("idCita")]
    public object? IdCita { get; set; }

    public DateTime? Fecha { get; set; }
    public string? Observaciones { get; set; }
    public string? NombreEstudiante { get; set; }
    public string? NumeroControl { get; set; }
    public string? Carrera { get; set; }
    public string? NombrePsicologo { get; set; }
    public string? HoraInicio { get; set; }
    public bool Asistio { get; set; } = true;
}

// Mantener compatibilidad con firmas anteriores
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

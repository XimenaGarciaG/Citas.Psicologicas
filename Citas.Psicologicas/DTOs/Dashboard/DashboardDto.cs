namespace Citas.Psicologicas.DTOs.Dashboard;

/// <summary>DTO con estadísticas devueltas por GET /dashboard</summary>
public class DashboardDto
{
    public int TotalCitas { get; set; }
    public int CitasReservadas { get; set; }
    public int CitasDisponibles { get; set; }
    public int CitasConcluidas { get; set; }
    public int CitasCanceladas { get; set; }
    public int TotalSesionesRegistradas { get; set; }
    public int TotalAsistencias { get; set; }
    public int TotalInasistencias { get; set; }
    public double PorcentajeAsistencia { get; set; }
    public int TotalSolicitudes { get; set; }
    public int SolicitudesPendientes { get; set; }
    public int TotalCanalizaciones { get; set; }

    public List<CitaMesDto> CitasPorMes { get; set; } = [];
    public List<SolicitudEstadoDto> SolicitudesPorEstado { get; set; } = [];
    public List<CitaProximaDto> ProximasCitas { get; set; } = [];
}

public class CitaMesDto
{
    public string Mes { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class SolicitudEstadoDto
{
    public string Estado { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class CitaProximaDto
{
    public string Id { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

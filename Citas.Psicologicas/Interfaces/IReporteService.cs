using Citas.Psicologicas.DTOs.Reportes;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Interfaces;

/// <summary>Contrato para el servicio de reportes administrativos</summary>
public interface IReporteService
{
    Task<ApiResponse<List<ReporteAsistenciaDto>>> GetAsistenciaAsync(DateTime inicio, DateTime fin, string token);
    Task<ApiResponse<List<ReporteInasistenciaDto>>> GetInasistenciaAsync(DateTime inicio, DateTime fin, string token);
    Task<byte[]> ExportarExcelAsistenciaAsync(List<ReporteAsistenciaDto> datos);
    Task<byte[]> ExportarExcelInasistenciaAsync(List<ReporteInasistenciaDto> datos);
}

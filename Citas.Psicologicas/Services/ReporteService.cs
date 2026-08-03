using ClosedXML.Excel;
using Citas.Psicologicas.Constants;
using Citas.Psicologicas.DTOs.Reportes;
using Citas.Psicologicas.Helpers;
using Citas.Psicologicas.Interfaces;

namespace Citas.Psicologicas.Services;

/// <summary>Servicio para generación y exportación de reportes administrativos</summary>
public class ReporteService : BaseApiService, IReporteService
{
    public ReporteService(IHttpClientFactory httpClientFactory, ILogger<ReporteService> logger)
        : base(httpClientFactory, logger) { }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ReporteAsistenciaDto>>> GetAsistenciaAsync(
        DateTime inicio, DateTime fin, string token)
    {
        var result = await GetAsync<ReporteRespuestaDto>(
            $"{ApiRoutes.ReportesAsistencia}?inicio={inicio:yyyy-MM-dd}&fin={fin:yyyy-MM-dd}", token);

        if (!result.Success)
            return ApiResponseHelper.Fail<List<ReporteAsistenciaDto>>(result.Message!, result.StatusCode);

        return ApiResponseHelper.Ok(MapAsistencia(result.Data?.Detalle ?? []));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ReporteInasistenciaDto>>> GetInasistenciaAsync(
        DateTime inicio, DateTime fin, string token)
    {
        var result = await GetAsync<ReporteRespuestaDto>(
            $"{ApiRoutes.ReportesInasistencia}?inicio={inicio:yyyy-MM-dd}&fin={fin:yyyy-MM-dd}", token);

        if (!result.Success)
            return ApiResponseHelper.Fail<List<ReporteInasistenciaDto>>(result.Message!, result.StatusCode);

        return ApiResponseHelper.Ok(MapInasistencia(result.Data?.Detalle ?? []));
    }

    /// <summary>Mapea el detalle del wrapper a la lista de asistencia</summary>
    private static List<ReporteAsistenciaDto> MapAsistencia(List<ReporteDetalleItemDto> detalle)
    {
        var list = new List<ReporteAsistenciaDto>(detalle.Count);
        foreach (var d in detalle)
        {
            list.Add(new ReporteAsistenciaDto
            {
                IdBitacora = d.IdBitacora,
                IdCita = d.IdCita,
                Fecha = d.Fecha,
                Observaciones = d.Observaciones,
                NombreEstudiante = d.NombreEstudiante,
                NumeroControl = d.NumeroControl,
                Carrera = d.Carrera,
                NombrePsicologo = d.NombrePsicologo,
                HoraInicio = d.HoraInicio,
                Asistio = d.Asistio
            });
        }
        return list;
    }

    /// <summary>Mapea el detalle del wrapper a la lista de inasistencias</summary>
    private static List<ReporteInasistenciaDto> MapInasistencia(List<ReporteDetalleItemDto> detalle)
    {
        var list = new List<ReporteInasistenciaDto>(detalle.Count);
        foreach (var d in detalle)
        {
            list.Add(new ReporteInasistenciaDto
            {
                IdBitacora = d.IdBitacora,
                IdCita = d.IdCita,
                Fecha = d.Fecha,
                Observaciones = d.Observaciones,
                NombreEstudiante = d.NombreEstudiante,
                NumeroControl = d.NumeroControl,
                Carrera = d.Carrera,
                NombrePsicologo = d.NombrePsicologo,
                HoraInicio = d.HoraInicio,
                Asistio = d.Asistio
            });
        }
        return list;
    }

    /// <summary>Exporta el reporte de asistencia a Excel usando ClosedXML</summary>
    public async Task<byte[]> ExportarExcelAsistenciaAsync(List<ReporteAsistenciaDto> datos)
    {
        return await Task.Run(() =>
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Reporte de Asistencia");

            // Encabezado institucional
            ws.Range(1, 1, 1, 8).Merge();
            ws.Cell(1, 1).Value = "UNIVERSIDAD TECNOLÓGICA DE TULA-TEPEJI – Reporte de Asistencia";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 8).Merge();
            ws.Cell(2, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

            // Cabeceras
            int headerRow = 4;
            var headers = new[] { "Estudiante", "No. Control", "Carrera", "Psicólogo/a", "Fecha", "Hora", "¿Asistió?", "Observaciones" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
            }

            var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            // Datos
            for (int i = 0; i < datos.Count; i++)
            {
                var item = datos[i];
                int row = headerRow + 1 + i;
                ws.Cell(row, 1).Value = item.NombreEstudiante;
                ws.Cell(row, 2).Value = item.NumeroControl;
                ws.Cell(row, 3).Value = item.Carrera;
                ws.Cell(row, 4).Value = item.NombrePsicologo;
                ws.Cell(row, 5).Value = item.Fecha?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 6).Value = item.HoraInicio;
                ws.Cell(row, 7).Value = item.Asistio ? "Sí" : "No";
                ws.Cell(row, 8).Value = item.Observaciones ?? "";

                if (i % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");

                var asistioCell = ws.Cell(row, 7);
                asistioCell.Style.Font.FontColor = item.Asistio ? XLColor.FromHtml("#059669") : XLColor.FromHtml("#DC2626");
                asistioCell.Style.Font.Bold = true;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        });
    }

    /// <summary>Exporta el reporte de inasistencias a Excel usando ClosedXML</summary>
    public async Task<byte[]> ExportarExcelInasistenciaAsync(List<ReporteInasistenciaDto> datos)
    {
        return await Task.Run(() =>
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Reporte de Inasistencias");

            ws.Range(1, 1, 1, 6).Merge();
            ws.Cell(1, 1).Value = "UNIVERSIDAD TECNOLÓGICA DE TULA-TEPEJI – Reporte de Inasistencias";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int headerRow = 4;
            var headers = new[] { "Estudiante", "No. Control", "Carrera", "Fecha Cita", "Hora", "Motivo" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(headerRow, i + 1).Value = headers[i];

            var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DC2626");

            for (int i = 0; i < datos.Count; i++)
            {
                var item = datos[i];
                int row = headerRow + 1 + i;
                ws.Cell(row, 1).Value = item.NombreEstudiante;
                ws.Cell(row, 2).Value = item.NumeroControl;
                ws.Cell(row, 3).Value = item.Carrera;
                ws.Cell(row, 4).Value = item.FechaCita.ToString("dd/MM/yyyy");
                ws.Cell(row, 5).Value = item.HoraInicio;
                ws.Cell(row, 6).Value = item.Motivo;

                if (i % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF2F2");
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        });
    }
}

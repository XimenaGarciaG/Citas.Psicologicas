namespace Citas.Psicologicas.DTOs.Bitacora;

/// <summary>DTO para registrar asistencia en POST /bitacora</summary>
public class CreateBitacoraDto
{
    public object IdCita { get; set; } = 0;
    public bool Asistencia { get; set; } = true;
    public string Observaciones { get; set; } = string.Empty;
    public bool AcuerdoSeguimiento { get; set; } = false;
}

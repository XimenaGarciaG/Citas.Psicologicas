namespace Citas.Psicologicas.Constants;

/// <summary>Orígenes válidos de una solicitud según el constraint CK__SOLICITUD__orige en la BD</summary>
public static class OrigenSolicitud
{
    /// <summary>Solicitud generada por el propio estudiante (autoservicio)</summary>
    public const string Autonoma = "AUTONOMA";
    /// <summary>Solicitud generada por el tutor del estudiante</summary>
    public const string Tutoria = "TUTORIA";
    /// <summary>Solicitud generada presencialmente por la psicóloga</summary>
    public const string Presencial = "PRESENCIAL";
}

/// <summary>Estados de una cita según la API REST</summary>
public static class EstadosCita
{
    public const string Reservada = "RESERVADA";
    public const string Confirmada = "CONFIRMADA";
    public const string Cancelada = "CANCELADA";
    public const string Concluida = "CONCLUIDA";
    public const string Reagendada = "REAGENDADA";
}

/// <summary>Estados de una solicitud de atención según la API REST</summary>
public static class EstadosSolicitud
{
    public const string Pendiente = "PENDIENTE";
    public const string Agendada = "AGENDADA";
    public const string Atendida = "ATENDIDA";
    public const string Cerrada = "CERRADA";
    public const string Cancelada = "CANCELADA";
}

/// <summary>Estados de una canalización según la API REST</summary>
public static class EstadosCanalizacion
{
    public const string Pendiente = "PENDIENTE";
    public const string Atendida = "ATENDIDA";
}

/// <summary>Prioridades de atención según la API REST</summary>
public static class Prioridades
{
    public const string Alta = "ALTA";
    public const string Media = "MEDIA";
    public const string Baja = "BAJA";
}

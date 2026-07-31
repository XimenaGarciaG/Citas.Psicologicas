namespace Citas.Psicologicas.Helpers;

/// <summary>Utilidades para formateo de fechas y horas</summary>
public static class DateHelper
{
    public static string FormatDate(DateTime date) => date.ToString("dd/MM/yyyy");
    public static string FormatDateTime(DateTime date) => date.ToString("dd/MM/yyyy HH:mm");
    public static string FormatTime(TimeSpan time) => time.ToString(@"hh\:mm");

    public static string RelativeTime(DateTime date)
    {
        var diff = DateTime.Now - date;
        if (diff.TotalDays >= 1) return $"hace {(int)diff.TotalDays} día(s)";
        if (diff.TotalHours >= 1) return $"hace {(int)diff.TotalHours} hora(s)";
        return $"hace {(int)diff.TotalMinutes} minuto(s)";
    }

    public static bool IsWorkingDay(DateTime date) =>
        date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
}

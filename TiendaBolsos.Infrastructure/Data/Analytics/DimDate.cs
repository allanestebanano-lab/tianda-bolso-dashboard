namespace TiendaBolsos.Infrastructure.Data.Analytics;

public sealed class DimDate
{
    public int DateKey { get; set; }
    public DateTime Date { get; set; }
    public short Year { get; set; }
    public byte Quarter { get; set; }
    public byte Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public byte Day { get; set; }
    public byte DayOfWeek { get; set; }
}

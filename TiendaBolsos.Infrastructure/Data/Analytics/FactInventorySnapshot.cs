namespace TiendaBolsos.Infrastructure.Data.Analytics;

public sealed class FactInventorySnapshot
{
    public int DateKey { get; set; }
    public int ProductKey { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
}

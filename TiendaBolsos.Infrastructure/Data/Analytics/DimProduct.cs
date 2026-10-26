namespace TiendaBolsos.Infrastructure.Data.Analytics;

public sealed class DimProduct
{
    public int ProductKey { get; set; }
    public int SourceProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

namespace TiendaBolsos.Infrastructure.Data.Analytics;

public sealed class FactSale
{
    public int SourceDetailId { get; set; }
    public int SourceSaleId { get; set; }
    public int DateKey { get; set; }
    public int ProductKey { get; set; }
    public string? PaymentMethod { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

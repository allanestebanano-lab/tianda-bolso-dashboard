namespace TiendaBolsos.Core.Modules.Analytics.DTOs;

public sealed class BusinessDashboardDto
{
    public decimal TotalSales { get; init; }

    public int TotalOrders { get; init; }

    public decimal AverageOrderValue { get; init; }

    public int UnitsSold { get; init; }

    public int LowStockProducts { get; init; }

    public IReadOnlyList<DailySalesKpiDto> DailySales { get; init; } = [];
}

public sealed class DailySalesKpiDto
{
    public DateTime Date { get; init; }

    public decimal Amount { get; init; }
}

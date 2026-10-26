using Microsoft.EntityFrameworkCore;
using TiendaBolsos.Core.Modules.Analytics.DTOs;
using TiendaBolsos.Core.Modules.Analytics.Interfaces;
using TiendaBolsos.Infrastructure.Data;

namespace TiendaBolsos.Infrastructure.Repositories;

public sealed class AnalyticsRepository(AnalyticsDbContext context) : IAnalyticsRepository
{
    public async Task<BusinessDashboardDto> GetBusinessDashboardAsync(
        CancellationToken cancellationToken)
    {
        var totalSales = await context.Sales.SumAsync(
            sale => (decimal?)sale.Amount, cancellationToken) ?? 0m;
        var totalOrders = await context.Sales
            .Select(sale => sale.SourceSaleId)
            .Distinct()
            .CountAsync(cancellationToken);
        var unitsSold = await context.Sales.SumAsync(
            sale => (int?)sale.Quantity, cancellationToken) ?? 0;

        var latestSnapshotDate = await context.InventorySnapshots
            .MaxAsync(snapshot => (int?)snapshot.DateKey, cancellationToken);
        var lowStockProducts = latestSnapshotDate is null
            ? 0
            : await context.InventorySnapshots.CountAsync(
                snapshot => snapshot.DateKey == latestSnapshotDate.Value &&
                            snapshot.StockQuantity <= snapshot.MinimumStock,
                cancellationToken);

        var startDate = DateTime.UtcNow.Date.AddDays(-6);
        var dailySales = await context.Sales
            .Join(context.Dates,
                sale => sale.DateKey,
                date => date.DateKey,
                (sale, date) => new { date.Date, sale.Amount })
            .Where(row => row.Date >= startDate)
            .GroupBy(row => row.Date)
            .Select(group => new DailySalesKpiDto
            {
                Date = group.Key,
                Amount = group.Sum(row => row.Amount)
            })
            .OrderBy(row => row.Date)
            .ToListAsync(cancellationToken);

        return new BusinessDashboardDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0m,
            UnitsSold = unitsSold,
            LowStockProducts = lowStockProducts,
            DailySales = dailySales
        };
    }

    public async Task<TechnicalDashboardDto> GetTechnicalDashboardAsync(
        CancellationToken cancellationToken)
    {
        var start = DateTime.UtcNow.AddHours(-24);
        var requests = context.ApiRequests.Where(request => request.TimestampUtc >= start);
        var totalRequests = await requests.CountAsync(cancellationToken);
        var serverErrors = await requests.CountAsync(
            request => request.IsServerError, cancellationToken);
        var averageResponse = await requests.AverageAsync(
            request => (decimal?)request.DurationMilliseconds, cancellationToken) ?? 0m;

        var dailyStart = DateTime.UtcNow.Date.AddDays(-6);
        var dailyRequests = await context.ApiRequests
            .Where(request => request.TimestampUtc >= dailyStart)
            .GroupBy(request => request.TimestampUtc.Date)
            .Select(group => new DailyRequestKpiDto
            {
                Date = group.Key,
                Requests = group.Count(),
                ServerErrors = group.Count(request => request.IsServerError)
            })
            .OrderBy(row => row.Date)
            .ToListAsync(cancellationToken);

        return new TechnicalDashboardDto
        {
            Requests = totalRequests,
            ServerErrors = serverErrors,
            ServerErrorRatePercent = totalRequests == 0
                ? 0m
                : decimal.Round((decimal)serverErrors * 100m / totalRequests, 2),
            AverageResponseMilliseconds = decimal.Round(averageResponse, 2),
            DailyRequests = dailyRequests
        };
    }
}

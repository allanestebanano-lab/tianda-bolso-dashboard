namespace TiendaBolsos.Core.Modules.Analytics.DTOs;

public sealed class TechnicalDashboardDto
{
    public int Requests { get; init; }

    public int ServerErrors { get; init; }

    public decimal ServerErrorRatePercent { get; init; }

    public decimal AverageResponseMilliseconds { get; init; }

    public IReadOnlyList<DailyRequestKpiDto> DailyRequests { get; init; } = [];
}

public sealed class DailyRequestKpiDto
{
    public DateTime Date { get; init; }

    public int Requests { get; init; }

    public int ServerErrors { get; init; }
}

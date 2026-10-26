namespace TiendaBolsos.Dashboard.Models;

public sealed class LoginModel
{
    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponseModel
{
    public string Token { get; set; } = string.Empty;
}

public sealed class BusinessDashboardModel
{
    public decimal TotalSales { get; set; }

    public int TotalOrders { get; set; }

    public decimal AverageOrderValue { get; set; }

    public int UnitsSold { get; set; }

    public int LowStockProducts { get; set; }

    public List<DailySalesModel> DailySales { get; set; } = [];
}

public sealed class DailySalesModel
{
    public DateTime Date { get; set; }

    public decimal Amount { get; set; }
}

public sealed class TechnicalDashboardModel
{
    public int Requests { get; set; }

    public int ServerErrors { get; set; }

    public decimal ServerErrorRatePercent { get; set; }

    public decimal AverageResponseMilliseconds { get; set; }

    public List<DailyRequestModel> DailyRequests { get; set; } = [];
}

public sealed class DailyRequestModel
{
    public DateTime Date { get; set; }

    public int Requests { get; set; }

    public int ServerErrors { get; set; }
}

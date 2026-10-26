namespace TiendaBolsos.Infrastructure.Data.Analytics;

public sealed class FactApiRequest
{
    public Guid RequestId { get; set; }
    public int DateKey { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public decimal DurationMilliseconds { get; set; }
    public bool IsServerError { get; set; }
    public string Environment { get; set; } = string.Empty;
}

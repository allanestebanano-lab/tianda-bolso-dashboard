namespace TiendaBolsos.API.Models;

public sealed class ApiTelemetry
{
    public string RequestId { get; init; } = string.Empty;

    public DateTime TimestampUtc { get; init; }

    public string HttpMethod { get; init; } = string.Empty;

    public string Route { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public decimal DurationMilliseconds { get; init; }

    public bool IsServerError { get; init; }

    public string Environment { get; init; } = string.Empty;
}

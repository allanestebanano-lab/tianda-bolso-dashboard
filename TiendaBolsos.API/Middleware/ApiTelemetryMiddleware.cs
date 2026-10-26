using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using TiendaBolsos.API.Models;

namespace TiendaBolsos.API.Middleware;

public sealed class ApiTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMongoCollection<ApiTelemetry> _collection;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ApiTelemetryMiddleware> _logger;

    public ApiTelemetryMiddleware(
        RequestDelegate next,
        IMongoCollection<ApiTelemetry> collection,
        IHostEnvironment environment,
        ILogger<ApiTelemetryMiddleware> logger)
    {
        _next = next;
        _collection = collection;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var route = (context.GetEndpoint() as RouteEndpoint)?
                .RoutePattern.RawText ?? "unmatched";

            var telemetry = new ApiTelemetry
            {
                RequestId = Guid.NewGuid().ToString("N"),
                TimestampUtc = DateTime.UtcNow,
                HttpMethod = context.Request.Method,
                Route = route,
                StatusCode = context.Response.StatusCode,
                DurationMilliseconds = (decimal)stopwatch.Elapsed.TotalMilliseconds,
                IsServerError = context.Response.StatusCode >= StatusCodes.Status500InternalServerError,
                Environment = _environment.EnvironmentName
            };

            try
            {
                await _collection.InsertOneAsync(telemetry, cancellationToken: context.RequestAborted);
            }
            catch (MongoException exception)
            {
                _logger.LogError(exception,
                    "Unable to persist API telemetry for request {RequestId}.",
                    telemetry.RequestId);
            }
            catch (OperationCanceledException exception)
                when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug(exception,
                    "API telemetry persistence was cancelled for request {RequestId}.",
                    telemetry.RequestId);
            }
        }
    }
}

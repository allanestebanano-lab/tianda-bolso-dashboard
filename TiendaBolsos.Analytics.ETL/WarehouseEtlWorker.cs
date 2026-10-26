using Microsoft.Data.SqlClient;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TiendaBolsos.Analytics.ETL;

public sealed class WarehouseEtlWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IMongoCollection<BsonDocument>? _telemetry;
    private readonly ILogger<WarehouseEtlWorker> _logger;
    private readonly TimeSpan _interval;

    public WarehouseEtlWorker(
        IConfiguration configuration,
        IServiceProvider services,
        ILogger<WarehouseEtlWorker> logger)
    {
        _configuration = configuration;
        _telemetry = services.GetService<IMongoCollection<BsonDocument>>();
        _logger = logger;
        _interval = TimeSpan.FromMinutes(
            configuration.GetValue<int?>("Etl:IntervalMinutes") ?? 5);

        if (_interval < TimeSpan.FromMinutes(1))
        {
            throw new InvalidOperationException("Etl:IntervalMinutes must be at least 1.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var operationalConnection = RequiredConnection("OperationalConnection");
        var warehouseConnection = RequiredConnection("WarehouseConnection");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(
                    operationalConnection,
                    warehouseConnection,
                    stoppingToken);
            }
            catch (SqlException exception)
            {
                _logger.LogError(exception, "The SQL Server ETL batch failed.");
            }
            catch (MongoException exception)
            {
                _logger.LogError(exception, "The MongoDB telemetry ETL batch failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(
        string operationalConnection,
        string warehouseConnection,
        CancellationToken cancellationToken)
    {
        await using var warehouse = new SqlConnection(warehouseConnection);
        await warehouse.OpenAsync(cancellationToken);
        await AssertWarehouseReadyAsync(warehouse, cancellationToken);

        await LoadProductsAsync(operationalConnection, warehouse, cancellationToken);
        await LoadSalesAsync(operationalConnection, warehouse, cancellationToken);
        await LoadInventorySnapshotAsync(operationalConnection, warehouse, cancellationToken);
        if (_telemetry is null)
        {
            _logger.LogWarning(
                "MongoDB telemetry was skipped. Set Mongo:ConnectionString to load technical KPIs.");
        }
        else
        {
            await LoadTelemetryAsync(warehouse, cancellationToken);
        }

        _logger.LogInformation("Data Warehouse ETL batch completed at {CompletedAtUtc}.",
            DateTime.UtcNow);
    }

    private async Task LoadProductsAsync(
        string operationalConnection,
        SqlConnection warehouse,
        CancellationToken cancellationToken)
    {
        const string sourceQuery = """
            SELECT p.IdProducto, p.Nombre, c.Nombre AS Categoria, m.Nombre AS Marca,
                   p.Precio, p.Activo
            FROM Catalogos.Producto AS p
            LEFT JOIN Catalogos.Categoria AS c ON c.IdCategoria = p.IdCategoria
            LEFT JOIN Catalogos.Marca AS m ON m.IdMarca = p.IdMarca;
            """;

        var products = new List<ProductRecord>();
        await using (var source = new SqlConnection(operationalConnection))
        {
            await source.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sourceQuery, source);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                products.Add(new ProductRecord(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "Sin categoría" : reader.GetString(2),
                    reader.IsDBNull(3) ? "Sin marca" : reader.GetString(3),
                    reader.GetDecimal(4),
                    reader.GetBoolean(5)));
            }
        }

        foreach (var product in products)
        {
            const string upsert = """
                UPDATE Analytics.DimProduct
                SET ProductName = @name, CategoryName = @category, BrandName = @brand,
                    UnitPrice = @price, IsActive = @active, UpdatedAtUtc = SYSUTCDATETIME()
                WHERE SourceProductId = @sourceId;
                IF @@ROWCOUNT = 0
                    INSERT INTO Analytics.DimProduct
                        (SourceProductId, ProductName, CategoryName, BrandName, UnitPrice, IsActive)
                    VALUES (@sourceId, @name, @category, @brand, @price, @active);
                """;

            await using var command = new SqlCommand(upsert, warehouse);
            command.Parameters.AddWithValue("@sourceId", product.SourceId);
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@category", product.Category);
            command.Parameters.AddWithValue("@brand", product.Brand);
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@active", product.IsActive);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task LoadSalesAsync(
        string operationalConnection,
        SqlConnection warehouse,
        CancellationToken cancellationToken)
    {
        const string pipelineName = "sales";
        var lastSourceId = await GetLastSourceIdAsync(
            warehouse, pipelineName, cancellationToken);
        const string sourceQuery = """
            SELECT d.IdDetalleVenta, d.IdVenta, v.Fecha, d.IdProducto,
                   v.MetodoPago, d.Cantidad, d.PrecioUnitario, d.Subtotal
            FROM Ventas.DetalleVenta AS d
            INNER JOIN Ventas.Venta AS v ON v.IdVenta = d.IdVenta
            WHERE d.IdDetalleVenta > @lastSourceId
            ORDER BY d.IdDetalleVenta;
            """;

        var sales = new List<SaleRecord>();
        await using (var source = new SqlConnection(operationalConnection))
        {
            await source.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sourceQuery, source);
            command.Parameters.AddWithValue("@lastSourceId", lastSourceId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                sales.Add(new SaleRecord(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetDateTime(2),
                    reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetInt32(5),
                    reader.GetDecimal(6),
                    reader.GetDecimal(7)));
            }
        }

        foreach (var sale in sales)
        {
            var dateKey = await EnsureDateAsync(warehouse, sale.Date, cancellationToken);
            var productKey = await GetProductKeyAsync(
                warehouse, sale.ProductId, cancellationToken);

            const string insertFact = """
                IF NOT EXISTS
                    (SELECT 1 FROM Analytics.FactSale WHERE SourceDetailId = @detailId)
                    INSERT INTO Analytics.FactSale
                        (SourceDetailId, SourceSaleId, DateKey, ProductKey, PaymentMethod,
                         Quantity, UnitPrice, Amount)
                    VALUES (@detailId, @saleId, @dateKey, @productKey, @paymentMethod,
                            @quantity, @unitPrice, @amount);
                """;
            await using var command = new SqlCommand(insertFact, warehouse);
            command.Parameters.AddWithValue("@detailId", sale.DetailId);
            command.Parameters.AddWithValue("@saleId", sale.SaleId);
            command.Parameters.AddWithValue("@dateKey", dateKey);
            command.Parameters.AddWithValue("@productKey", productKey);
            command.Parameters.AddWithValue("@paymentMethod", (object?)sale.PaymentMethod ?? DBNull.Value);
            command.Parameters.AddWithValue("@quantity", sale.Quantity);
            command.Parameters.AddWithValue("@unitPrice", sale.UnitPrice);
            command.Parameters.AddWithValue("@amount", sale.Amount);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        if (sales.Count > 0)
        {
            await SetLastSourceIdAsync(
                warehouse, pipelineName, sales[^1].DetailId, cancellationToken);
        }
    }

    private async Task LoadInventorySnapshotAsync(
        string operationalConnection,
        SqlConnection warehouse,
        CancellationToken cancellationToken)
    {
        const string sourceQuery = """
            SELECT IdProducto, Stock, MinStock
            FROM Catalogos.Inventory;
            """;
        var stockRows = new List<InventoryRecord>();

        await using (var source = new SqlConnection(operationalConnection))
        {
            await source.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sourceQuery, source);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                stockRows.Add(new InventoryRecord(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2)));
            }
        }

        var dateKey = await EnsureDateAsync(warehouse, DateTime.UtcNow, cancellationToken);
        foreach (var row in stockRows)
        {
            var productKey = await GetProductKeyAsync(
                warehouse, row.ProductId, cancellationToken);
            const string upsert = """
                UPDATE Analytics.FactInventorySnapshot
                SET StockQuantity = @stock, MinimumStock = @minimum
                WHERE DateKey = @dateKey AND ProductKey = @productKey;
                IF @@ROWCOUNT = 0
                    INSERT INTO Analytics.FactInventorySnapshot
                        (DateKey, ProductKey, StockQuantity, MinimumStock)
                    VALUES (@dateKey, @productKey, @stock, @minimum);
                """;
            await using var command = new SqlCommand(upsert, warehouse);
            command.Parameters.AddWithValue("@dateKey", dateKey);
            command.Parameters.AddWithValue("@productKey", productKey);
            command.Parameters.AddWithValue("@stock", row.Stock);
            command.Parameters.AddWithValue("@minimum", row.MinimumStock);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task LoadTelemetryAsync(
        SqlConnection warehouse,
        CancellationToken cancellationToken)
    {
        const string pipelineName = "api-telemetry";
        var lastTimestamp = await GetLastTimestampAsync(
            warehouse, pipelineName, cancellationToken);
        var filter = lastTimestamp is null
            ? FilterDefinition<BsonDocument>.Empty
            : Builders<BsonDocument>.Filter.Gte(
                "TimestampUtc", lastTimestamp.Value.AddMinutes(-1));

        DateTime? latestTimestamp = lastTimestamp;
        var options = new FindOptions<BsonDocument>
        {
            BatchSize = 500,
            Sort = Builders<BsonDocument>.Sort.Ascending("TimestampUtc")
        };
        using var cursor = await _telemetry.FindAsync(
            filter, options, cancellationToken);
        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var document in cursor.Current)
            {
                var requestId = ReadGuid(document, "RequestId");
                var timestamp = ReadDateTime(document, "TimestampUtc");
                var dateKey = await EnsureDateAsync(warehouse, timestamp, cancellationToken);
                const string insertFact = """
                    IF NOT EXISTS
                        (SELECT 1 FROM Analytics.FactApiRequest WHERE RequestId = @requestId)
                        INSERT INTO Analytics.FactApiRequest
                            (RequestId, DateKey, TimestampUtc, HttpMethod, Route, StatusCode,
                             DurationMilliseconds, IsServerError, Environment)
                        VALUES (@requestId, @dateKey, @timestamp, @method, @route, @status,
                                @duration, @isError, @environment);
                    """;
                await using var command = new SqlCommand(insertFact, warehouse);
                command.Parameters.AddWithValue("@requestId", requestId);
                command.Parameters.AddWithValue("@dateKey", dateKey);
                command.Parameters.AddWithValue("@timestamp", timestamp);
                command.Parameters.AddWithValue("@method", ReadString(document, "HttpMethod"));
                command.Parameters.AddWithValue("@route", ReadString(document, "Route"));
                command.Parameters.AddWithValue("@status", ReadInt32(document, "StatusCode"));
                command.Parameters.AddWithValue("@duration", ReadDecimal(document, "DurationMilliseconds"));
                command.Parameters.AddWithValue("@isError", ReadBoolean(document, "IsServerError"));
                command.Parameters.AddWithValue("@environment", ReadString(document, "Environment"));
                await command.ExecuteNonQueryAsync(cancellationToken);

                if (latestTimestamp is null || timestamp > latestTimestamp)
                {
                    latestTimestamp = timestamp;
                }
            }
        }

        if (latestTimestamp is not null && latestTimestamp > lastTimestamp)
        {
            await SetLastTimestampAsync(
                warehouse, pipelineName, latestTimestamp.Value, cancellationToken);
        }
    }

    private static async Task<int> EnsureDateAsync(
        SqlConnection warehouse,
        DateTime value,
        CancellationToken cancellationToken)
    {
        var date = value.Date;
        var dateKey = int.Parse(date.ToString("yyyyMMdd"));
        const string upsert = """
            IF NOT EXISTS (SELECT 1 FROM Analytics.DimDate WHERE DateKey = @dateKey)
                INSERT INTO Analytics.DimDate
                    (DateKey, [Date], [Year], [Quarter], [Month], MonthName, [Day], DayOfWeek)
                VALUES
                    (@dateKey, @date, @year, @quarter, @month, @monthName, @day, @dayOfWeek);
            """;
        await using var command = new SqlCommand(upsert, warehouse);
        command.Parameters.AddWithValue("@dateKey", dateKey);
        command.Parameters.AddWithValue("@date", date);
        command.Parameters.AddWithValue("@year", date.Year);
        command.Parameters.AddWithValue("@quarter", (date.Month - 1) / 3 + 1);
        command.Parameters.AddWithValue("@month", date.Month);
        command.Parameters.AddWithValue("@monthName", date.ToString("MMMM"));
        command.Parameters.AddWithValue("@day", date.Day);
        command.Parameters.AddWithValue("@dayOfWeek", (int)date.DayOfWeek);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return dateKey;
    }

    private static async Task<int> GetProductKeyAsync(
        SqlConnection warehouse,
        int sourceProductId,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT ProductKey FROM Analytics.DimProduct
            WHERE SourceProductId = @sourceProductId;
            """;
        await using var command = new SqlCommand(query, warehouse);
        command.Parameters.AddWithValue("@sourceProductId", sourceProductId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int key
            ? key
            : throw new InvalidOperationException(
                $"Source product {sourceProductId} is missing from Analytics.DimProduct.");
    }

    private static async Task AssertWarehouseReadyAsync(
        SqlConnection warehouse,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT CASE WHEN OBJECT_ID(N'Analytics.FactSale', N'U') IS NOT NULL
                              AND OBJECT_ID(N'Analytics.FactApiRequest', N'U') IS NOT NULL
                        THEN 1 ELSE 0 END;
            """;
        await using var command = new SqlCommand(query, warehouse);
        var isReady = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        if (isReady != 1)
        {
            throw new InvalidOperationException(
                "The Data Warehouse is not initialized. Run sql/DataWarehouse.sql first.");
        }
    }

    private static async Task<long> GetLastSourceIdAsync(
        SqlConnection warehouse,
        string pipelineName,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT LastSourceId FROM Analytics.EtlWatermark
            WHERE PipelineName = @pipelineName;
            """;
        await using var command = new SqlCommand(query, warehouse);
        command.Parameters.AddWithValue("@pipelineName", pipelineName);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long value ? value : 0L;
    }

    private static async Task<DateTime?> GetLastTimestampAsync(
        SqlConnection warehouse,
        string pipelineName,
        CancellationToken cancellationToken)
    {
        const string query = """
            SELECT LastTimestampUtc FROM Analytics.EtlWatermark
            WHERE PipelineName = @pipelineName;
            """;
        await using var command = new SqlCommand(query, warehouse);
        command.Parameters.AddWithValue("@pipelineName", pipelineName);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is DateTime value ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : null;
    }

    private static async Task SetLastSourceIdAsync(
        SqlConnection warehouse,
        string pipelineName,
        long lastSourceId,
        CancellationToken cancellationToken)
    {
        const string upsert = """
            UPDATE Analytics.EtlWatermark
            SET LastSourceId = @sourceId, UpdatedAtUtc = SYSUTCDATETIME()
            WHERE PipelineName = @pipelineName;
            IF @@ROWCOUNT = 0
                INSERT INTO Analytics.EtlWatermark (PipelineName, LastSourceId)
                VALUES (@pipelineName, @sourceId);
            """;
        await using var command = new SqlCommand(upsert, warehouse);
        command.Parameters.AddWithValue("@pipelineName", pipelineName);
        command.Parameters.AddWithValue("@sourceId", lastSourceId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SetLastTimestampAsync(
        SqlConnection warehouse,
        string pipelineName,
        DateTime lastTimestamp,
        CancellationToken cancellationToken)
    {
        const string upsert = """
            UPDATE Analytics.EtlWatermark
            SET LastTimestampUtc = @timestamp, UpdatedAtUtc = SYSUTCDATETIME()
            WHERE PipelineName = @pipelineName;
            IF @@ROWCOUNT = 0
                INSERT INTO Analytics.EtlWatermark (PipelineName, LastTimestampUtc)
                VALUES (@pipelineName, @timestamp);
            """;
        await using var command = new SqlCommand(upsert, warehouse);
        command.Parameters.AddWithValue("@pipelineName", pipelineName);
        command.Parameters.AddWithValue("@timestamp", lastTimestamp);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string RequiredConnection(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);
        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(
                $"ConnectionStrings:{name} is required in the ETL configuration.");
    }

    private static Guid ReadGuid(BsonDocument document, string name)
    {
        var value = document[name];
        return value.IsString
            ? Guid.Parse(value.AsString)
            : value.AsGuid;
    }

    private static DateTime ReadDateTime(BsonDocument document, string name)
    {
        var value = document[name];
        return value.IsBsonDateTime
            ? value.AsBsonDateTime.ToUniversalTime()
            : throw new InvalidOperationException(
                $"MongoDB telemetry field '{name}' must be a BSON date.");
    }

    private static string ReadString(BsonDocument document, string name) =>
        document.TryGetValue(name, out var value) && value.IsString
            ? value.AsString
            : throw new InvalidOperationException(
                $"MongoDB telemetry field '{name}' must be a string.");

    private static int ReadInt32(BsonDocument document, string name) =>
        document[name].ToInt32();

    private static decimal ReadDecimal(BsonDocument document, string name) =>
        document[name].ToDecimal();

    private static bool ReadBoolean(BsonDocument document, string name) =>
        document[name].ToBoolean();

    private sealed record ProductRecord(
        int SourceId,
        string Name,
        string Category,
        string Brand,
        decimal Price,
        bool IsActive);

    private sealed record SaleRecord(
        int DetailId,
        int SaleId,
        DateTime Date,
        int ProductId,
        string? PaymentMethod,
        int Quantity,
        decimal UnitPrice,
        decimal Amount);

    private sealed record InventoryRecord(int ProductId, int Stock, int MinimumStock);
}

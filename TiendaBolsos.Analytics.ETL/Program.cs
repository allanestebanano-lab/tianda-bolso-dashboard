using TiendaBolsos.Analytics.ETL;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);

var mongoConnectionString = builder.Configuration["Mongo:ConnectionString"];
if (string.IsNullOrWhiteSpace(mongoConnectionString))
{
    throw new InvalidOperationException(
        "Set Mongo:ConnectionString using user-secrets or the MONGO__CONNECTIONSTRING environment variable.");
}

var mongoDatabaseName = builder.Configuration["Mongo:DatabaseName"];
var telemetryCollectionName = builder.Configuration["Mongo:TelemetryCollection"];
if (string.IsNullOrWhiteSpace(mongoDatabaseName) ||
    string.IsNullOrWhiteSpace(telemetryCollectionName))
{
    throw new InvalidOperationException(
        "Set Mongo:DatabaseName and Mongo:TelemetryCollection in configuration.");
}

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoDatabase>()
        .GetCollection<MongoDB.Bson.BsonDocument>(telemetryCollectionName));
builder.Services.AddHostedService<WarehouseEtlWorker>();

var host = builder.Build();
host.Run();

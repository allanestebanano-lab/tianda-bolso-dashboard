using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using TiendaBolsos.Infrastructure.Data;
using TiendaBolsos.Infrastructure.Repositories;
using TiendaBolsos.Shared.JWT;

using TiendaBolsos.Core.Modules.Products.Interfaces;
using TiendaBolsos.Core.Modules.Products.Services;
using TiendaBolsos.Core.Modules.Inventory.Interfaces;
using TiendaBolsos.Core.Modules.Inventory.Services;
using TiendaBolsos.Core.Modules.Reports.Interfaces;
using TiendaBolsos.Core.Modules.Reports.Services;
using TiendaBolsos.Core.Modules.Sales.Interfaces;
using TiendaBolsos.Core.Modules.Sales.Services;

using TiendaBolsos.Core.Modules.Auth.Interfaces;
using TiendaBolsos.Core.Modules.Auth.Services;
using TiendaBolsos.API.Middleware;
using TiendaBolsos.API.Models;
using MongoDB.Driver;
using TiendaBolsos.Core.Modules.Analytics.Interfaces;
using TiendaBolsos.Core.Modules.Analytics.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE
// =========================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WarehouseConnection")));

var mongoConnectionString = builder.Configuration["Mongo:ConnectionString"];
if (!string.IsNullOrWhiteSpace(mongoConnectionString))
{
    var mongoClient = new MongoClient(mongoConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(
        builder.Configuration["Mongo:DatabaseName"] ?? "TiendaBolsos");
    var telemetryCollectionName =
        builder.Configuration["Mongo:TelemetryCollection"] ?? "apiTelemetry";

    builder.Services.AddSingleton(mongoClient);
    builder.Services.AddSingleton(
        mongoDatabase.GetCollection<ApiTelemetry>(telemetryCollectionName));
}

// =========================
// JWT
// =========================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>();

var key =
    Encoding.UTF8.GetBytes(jwtSettings!.Key);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,

                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };
    });

// =========================
// SERVICES
// =========================

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<JwtGenerator>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<ISalesRepository, SalesRepository>();

builder.Services.AddScoped<ISalesService, SalesService>();

builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// =========================
// CONTROLLERS
// =========================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });
// =========================
// SWAGGER
// =========================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new()
        {
            Title = "TiendaBolsos API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                Microsoft.OpenApi.Models.SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = Microsoft.OpenApi.Models.ParameterLocation.Header,

            Description =
                "Ingrese el token JWT"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models.ReferenceType
                                    .SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// =========================
// CORS
// =========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// =========================
// MIDDLEWARE
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

if (!string.IsNullOrWhiteSpace(mongoConnectionString))
{
    app.UseMiddleware<ApiTelemetryMiddleware>();
}
else
{
    app.Logger.LogWarning(
        "MongoDB telemetry is disabled. Configure Mongo:ConnectionString to enable it.");
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
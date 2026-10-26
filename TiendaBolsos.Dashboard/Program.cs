using TiendaBolsos.Dashboard.Components;
using TiendaBolsos.Dashboard.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddHttpClient("TiendaBolsosApi", client =>
{
    var baseAddress = builder.Configuration["Api:BaseAddress"];
    if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var apiAddress))
    {
        throw new InvalidOperationException(
            "Set Api:BaseAddress to the absolute URL of TiendaBolsos.API.");
    }

    client.BaseAddress = apiAddress;
});
builder.Services.AddScoped<DashboardApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

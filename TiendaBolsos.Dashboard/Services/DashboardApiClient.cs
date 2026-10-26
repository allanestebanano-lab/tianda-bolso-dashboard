using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TiendaBolsos.Dashboard.Models;

namespace TiendaBolsos.Dashboard.Services;

public sealed class DashboardApiClient(IHttpClientFactory httpClientFactory)
{
    public string? Token { get; set; }

    public async Task<string> LoginAsync(LoginModel credentials)
    {
        using var client = httpClientFactory.CreateClient("TiendaBolsosApi");
        using var response = await client.PostAsJsonAsync("api/auth/login", credentials);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateRequestExceptionAsync(response, "login");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
        return !string.IsNullOrWhiteSpace(result?.Token)
            ? result.Token
            : throw new InvalidOperationException("La API no devolvió un token de acceso.");
    }

    public async Task<(BusinessDashboardModel Business, TechnicalDashboardModel Technical)>
        GetDashboardAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            throw new InvalidOperationException("Debes iniciar sesión antes de cargar el dashboard.");
        }

        var businessTask = GetAsync<BusinessDashboardModel>("api/analytics/business");
        var technicalTask = GetAsync<TechnicalDashboardModel>("api/analytics/technical");
        await Task.WhenAll(businessTask, technicalTask);
        return (await businessTask, await technicalTask);
    }

    private async Task<T> GetAsync<T>(string path)
    {
        using var client = httpClientFactory.CreateClient("TiendaBolsosApi");
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateRequestExceptionAsync(response, path);
        }

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException($"La API devolvió una respuesta vacía para {path}.");
    }

    private static async Task<HttpRequestException> CreateRequestExceptionAsync(
        HttpResponseMessage response,
        string operation)
    {
        var statusCode = response.StatusCode;
        var apiMessage = await response.Content.ReadAsStringAsync();
        var detail = statusCode == HttpStatusCode.Unauthorized
            ? "Verifica las credenciales."
            : statusCode == HttpStatusCode.Forbidden
                ? "La cuenta debe tener rol Admin o Gerente."
                : string.IsNullOrWhiteSpace(apiMessage)
                    ? response.ReasonPhrase
                    : apiMessage;

        return new HttpRequestException(
            $"La API rechazó '{operation}' ({(int)statusCode}): {detail}",
            null,
            statusCode);
    }
}

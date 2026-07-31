using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Citas.Psicologicas.Helpers;

namespace Citas.Psicologicas.Services;

/// <summary>Clase base para servicios que consumen la API REST mediante HttpClientFactory</summary>
public abstract class BaseApiService
{
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly ILogger Logger;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected BaseApiService(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        HttpClientFactory = httpClientFactory;
        Logger = logger;
    }

    /// <summary>Crea un HttpClient con el token Bearer configurado</summary>
    protected HttpClient CreateClient(string? token = null)
    {
        var client = HttpClientFactory.CreateClient("ApiClient");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Realiza una solicitud GET y deserializa la respuesta</summary>
    protected async Task<ApiResponse<T>> GetAsync<T>(string url, string token)
    {
        try
        {
            var client = CreateClient(token);
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, JsonOptions);
                return ApiResponseHelper.Ok(data!);
            }
            Logger.LogWarning("GET {Url} -> {Status}: {Body}", url, response.StatusCode, content);
            return ApiResponseHelper.Fail<T>($"Error del servidor: {response.StatusCode}", response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Error de red en GET {Url}", url);
            return ApiResponseHelper.Fail<T>("No se pudo conectar con el servidor. Verifique la conexión.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error inesperado en GET {Url}", url);
            return ApiResponseHelper.Fail<T>("Error inesperado. Intente nuevamente.");
        }
    }

    /// <summary>Realiza una solicitud POST y deserializa la respuesta</summary>
    protected async Task<ApiResponse<T>> PostAsync<T>(string url, object body, string? token = null)
    {
        try
        {
            var client = CreateClient(token);
            var json = JsonSerializer.Serialize(body, JsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
                return ApiResponseHelper.Ok(data!);
            }
            Logger.LogWarning("POST {Url} -> {Status}: {Body}", url, response.StatusCode, responseContent);
            return ApiResponseHelper.Fail<T>($"Error del servidor: {response.StatusCode}", response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Error de red en POST {Url}", url);
            return ApiResponseHelper.Fail<T>("No se pudo conectar con el servidor.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error inesperado en POST {Url}", url);
            return ApiResponseHelper.Fail<T>("Error inesperado. Intente nuevamente.");
        }
    }

    /// <summary>Realiza una solicitud PUT y deserializa la respuesta</summary>
    protected async Task<ApiResponse<T>> PutAsync<T>(string url, object body, string token)
    {
        try
        {
            var client = CreateClient(token);
            var json = JsonSerializer.Serialize(body, JsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
                return ApiResponseHelper.Ok(data!);
            }
            return ApiResponseHelper.Fail<T>($"Error del servidor: {response.StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error en PUT {Url}", url);
            return ApiResponseHelper.Fail<T>("Error inesperado. Intente nuevamente.");
        }
    }

    /// <summary>Realiza una solicitud PATCH y deserializa la respuesta</summary>
    protected async Task<ApiResponse<T>> PatchAsync<T>(string url, object? body, string token)
    {
        try
        {
            var client = CreateClient(token);
            HttpContent? httpContent = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await client.PatchAsync(url, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
                return ApiResponseHelper.Ok(data!);
            }
            return ApiResponseHelper.Fail<T>($"Error del servidor: {response.StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error en PATCH {Url}", url);
            return ApiResponseHelper.Fail<T>("Error inesperado. Intente nuevamente.");
        }
    }
}

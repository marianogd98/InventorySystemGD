// Inventory.Web/Services/InventoryApiService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Inventory.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Inventory.Web.Services;

/// <summary>
/// Respuesta del endpoint de autenticación con el token JWT emitido.
/// </summary>
public record AuthResponse(string Token, string TokenType, DateTime ExpiresAt);

/// <summary>
/// DTO para transferir información de productos al cliente web.
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    DateTime CreatedAt
);

/// <summary>
/// Contrato del servicio HTTP cliente para comunicar la capa Web con la API REST.
/// </summary>
public interface IInventoryApiService
{
    Task<(bool Success, string? Token, string? ErrorMessage)> LoginAsync(string username, string password);
    Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync();
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
    Task<(bool Success, string? ErrorMessage)> CreateProductAsync(string name, string category, decimal price, int stock);
}

/// <summary>
/// Implementación del servicio cliente usando Typed HttpClient.
/// Administra la autenticación JWT automática y reenvía el token de la sesión activa del usuario.
/// </summary>
public class InventoryApiService : IInventoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<InventoryApiService> _logger;
    private string? _cachedToken;

    public InventoryApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<InventoryApiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene el token JWT del contexto de sesión del usuario actual o de la caché interna.
    /// </summary>
    private string? GetCurrentJwtToken()
    {
        // 1. Intentar obtener el token de los claims del usuario autenticado en la cookie
        var claimToken = _httpContextAccessor.HttpContext?.User?.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrWhiteSpace(claimToken))
        {
            return claimToken;
        }

        // 2. Si no está en claims, usar el token en memoria
        return _cachedToken;
    }

    /// <summary>
    /// Autentica un usuario contra el endpoint /api/auth/login y devuelve el token JWT emitido.
    /// </summary>
    public async Task<(bool Success, string? Token, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        try
        {
            var loginPayload = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginPayload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al autenticar con la API. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                return (false, null, "Credenciales incorrectas o servidor no disponible.");
            }

            var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (authResult is not null && !string.IsNullOrWhiteSpace(authResult.Token))
            {
                _cachedToken = authResult.Token;
                return (true, authResult.Token, null);
            }

            return (false, null, "La respuesta de la API no contiene un token válido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la autenticación con la API.");
            return (false, null, $"Error al conectar con la API: {ex.Message}");
        }
    }

    private async Task<string?> EnsureTokenAsync()
    {
        var token = GetCurrentJwtToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            var (_, fallbackToken, _) = await LoginAsync("admin", "admin123");
            token = fallbackToken;
        }
        return token;
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateProductAsync(string name, string category, decimal price, int stock)
    {
        try
        {
            var token = await EnsureTokenAsync();

            var payload = new { Name = name, Category = category, Price = price, Stock = stock };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
            {
                Content = JsonContent.Create(payload)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var (_, newToken, _) = await LoginAsync("admin", "admin123");
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Post, "/api/products")
                    {
                        Content = JsonContent.Create(payload)
                    };
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await _httpClient.SendAsync(retryRequest);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error al crear producto. Status: {StatusCode}, Respuesta: {Error}", response.StatusCode, errorBody);
            return (false, $"Error {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al conectar con la API para crear producto.");
            return (false, $"No se pudo comunicar con la API en '{_httpClient.BaseAddress}': {ex.Message}");
        }
    }

    public async Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync()
    {
        try
        {
            var token = await EnsureTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/inventory-value-by-category");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var (_, newToken, _) = await LoginAsync("admin", "admin123");
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/inventory-value-by-category");
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await _httpClient.SendAsync(retryRequest);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener valor de inventario por categoría. Status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CategoryInventoryValue>();
            }

            var data = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryInventoryValue>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data ?? Enumerable.Empty<CategoryInventoryValue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar el valor del inventario por categoría.");
            return Enumerable.Empty<CategoryInventoryValue>();
        }
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            var token = await EnsureTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/low-stock?threshold={threshold}");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var (_, newToken, _) = await LoginAsync("admin", "admin123");
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/products/low-stock?threshold={threshold}");
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await _httpClient.SendAsync(retryRequest);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener productos con bajo stock. Status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<ProductDto>();
            }

            var data = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data ?? Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al consultar productos con bajo stock.");
            return Enumerable.Empty<ProductDto>();
        }
    }
}

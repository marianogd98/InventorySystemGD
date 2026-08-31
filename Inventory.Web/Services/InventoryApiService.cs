// Inventory.Web/Services/InventoryApiService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Inventory.Domain.Models;

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
    Task<bool> AuthenticateAsync(string username = "admin", string password = "admin123");
    Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync();
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
    Task<(bool Success, string? ErrorMessage)> CreateProductAsync(string name, string category, decimal price, int stock);
}

/// <summary>
/// Implementación del servicio cliente usando Typed HttpClient.
/// Administra la autenticación JWT automática y reintentos ante respuestas 401 Unauthorized.
/// </summary>
public class InventoryApiService : IInventoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiService> _logger;
    private string? _jwtToken;

    public InventoryApiService(HttpClient httpClient, ILogger<InventoryApiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> AuthenticateAsync(string username = "admin", string password = "admin123")
    {
        try
        {
            var loginPayload = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginPayload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al autenticar con la API. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                return false;
            }

            var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (authResult is not null && !string.IsNullOrWhiteSpace(authResult.Token))
            {
                _jwtToken = authResult.Token;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la autenticación con la API.");
            return false;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (string.IsNullOrWhiteSpace(_jwtToken))
        {
            await AuthenticateAsync();
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateProductAsync(string name, string category, decimal price, int stock)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var payload = new { Name = name, Category = category, Price = price, Stock = stock };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
            {
                Content = JsonContent.Create(payload)
            };

            if (!string.IsNullOrWhiteSpace(_jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await AuthenticateAsync())
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Post, "/api/products")
                    {
                        Content = JsonContent.Create(payload)
                    };
                    if (!string.IsNullOrWhiteSpace(_jwtToken))
                    {
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    }
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
            await EnsureAuthenticatedAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/inventory-value-by-category");
            if (!string.IsNullOrWhiteSpace(_jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await AuthenticateAsync())
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Get, "/api/products/inventory-value-by-category");
                    if (!string.IsNullOrWhiteSpace(_jwtToken))
                    {
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    }
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
            await EnsureAuthenticatedAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/low-stock?threshold={threshold}");
            if (!string.IsNullOrWhiteSpace(_jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await AuthenticateAsync())
                {
                    using var retryRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/products/low-stock?threshold={threshold}");
                    if (!string.IsNullOrWhiteSpace(_jwtToken))
                    {
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    }
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

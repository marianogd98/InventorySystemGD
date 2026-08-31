// Inventory.Web/Services/InventoryApiService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Inventory.Domain.Models;

namespace Inventory.Web.Services;

public record AuthResponse(string Token, string TokenType, DateTime ExpiresAt);

public record ProductDto(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    DateTime CreatedAt
);

public interface IInventoryApiService
{
    Task<bool> AuthenticateAsync(string username = "admin", string password = "admin123");
    Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync();
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
}

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
                _logger.LogWarning("Error al autenticar con la API. Código de estado: {StatusCode}", response.StatusCode);
                return false;
            }

            var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (authResult is not null && !string.IsNullOrWhiteSpace(authResult.Token))
            {
                _jwtToken = authResult.Token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _jwtToken);
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

    public async Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync()
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var response = await _httpClient.GetAsync("/api/products/inventory-value-by-category");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Reintentar tras renovar token
                if (await AuthenticateAsync())
                {
                    response = await _httpClient.GetAsync("/api/products/inventory-value-by-category");
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

            var response = await _httpClient.GetAsync($"/api/products/low-stock?threshold={threshold}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Reintentar tras renovar token
                if (await AuthenticateAsync())
                {
                    response = await _httpClient.GetAsync($"/api/products/low-stock?threshold={threshold}");
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


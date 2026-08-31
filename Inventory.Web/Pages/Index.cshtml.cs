// Inventory.Web/Pages/Index.cshtml.cs
using Inventory.Domain.Models;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventory.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IInventoryApiService _apiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IInventoryApiService apiService, ILogger<IndexModel> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IEnumerable<CategoryInventoryValue> InventorySummary { get; private set; } = Enumerable.Empty<CategoryInventoryValue>();
    public IEnumerable<ProductDto> LowStockProducts { get; private set; } = Enumerable.Empty<ProductDto>();

    public decimal TotalGlobalValue => InventorySummary.Sum(x => x.TotalInventoryValue);
    public int TotalGlobalUnits => InventorySummary.Sum(x => x.TotalUnits);
    public int TotalGlobalProducts => InventorySummary.Sum(x => x.ProductCount);

    public async Task OnGetAsync()
    {
        try
        {
            var summaryTask = _apiService.GetInventoryValueByCategoryAsync();
            var lowStockTask = _apiService.GetLowStockProductsAsync(threshold: 10);

            await Task.WhenAll(summaryTask, lowStockTask);

            InventorySummary = await summaryTask;
            LowStockProducts = await lowStockTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los datos del inventario en la página principal.");
        }
    }
}

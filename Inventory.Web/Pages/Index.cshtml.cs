// Inventory.Web/Pages/Index.cshtml.cs
using System.ComponentModel.DataAnnotations;
using Inventory.Domain.Models;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;
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

    [BindProperty]
    public CreateProductInput NewProduct { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? StatusType { get; set; }

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

    public async Task<IActionResult> OnPostCreateProductAsync()
    {
        if (!ModelState.IsValid)
        {
            StatusMessage = "Por favor, verifica los campos ingresados.";
            StatusType = "danger";
            await OnGetAsync();
            return Page();
        }

        var (success, errorMessage) = await _apiService.CreateProductAsync(
            name: NewProduct.Name,
            category: NewProduct.Category,
            price: NewProduct.Price,
            stock: NewProduct.Stock
        );

        if (success)
        {
            StatusMessage = $"¡Producto '{NewProduct.Name}' registrado exitosamente!";
            StatusType = "success";
        }
        else
        {
            StatusMessage = !string.IsNullOrEmpty(errorMessage)
                ? $"No se pudo registrar el producto: {errorMessage}"
                : "No se pudo registrar el producto. Verifica que la API esté activa y las credenciales sean correctas.";
            StatusType = "danger";
        }

        return RedirectToPage();
    }
}

public class CreateProductInput
{
    private const string SafeTextPattern = @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s.,_/#()\-]+$";
    private const string SafeTextErrorMessage = "No se permiten caracteres especiales que puedan usarse para inyecciones SQL (como comillas, punto y coma, comentarios, etc.).";

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    [RegularExpression(SafeTextPattern, ErrorMessage = "El nombre contiene caracteres especiales no permitidos.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Categoría")]
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres.")]
    [RegularExpression(SafeTextPattern, ErrorMessage = "La categoría contiene caracteres especiales no permitidos.")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Precio")]
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 1000000.00, ErrorMessage = "El precio debe ser mayor a 0 y no puede ser negativo.")]
    public decimal Price { get; set; }

    [Display(Name = "Stock inicial")]
    [Required(ErrorMessage = "El stock inicial es obligatorio.")]
    [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo (debe ser 0 o mayor).")]
    public int Stock { get; set; }
}

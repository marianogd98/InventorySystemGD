// Inventory.Web/Pages/Index.cshtml.cs
using System.ComponentModel.DataAnnotations;
using Inventory.Domain.Models;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventory.Web.Pages;

/// <summary>
/// Modelo de página Razor (PageModel) para el panel principal de inventario.
/// Gestiona la visualización de métricas analíticas, tablas y procesamiento del formulario de creación.
/// </summary>
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

    // Propiedades TempData para persistir el estado de alertas y disparar SweetAlert2 tras redirecciones
    [TempData]
    public string? StatusTitle { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? StatusType { get; set; }

    public IEnumerable<CategoryInventoryValue> InventorySummary { get; private set; } = Enumerable.Empty<CategoryInventoryValue>();
    public IEnumerable<ProductDto> LowStockProducts { get; private set; } = Enumerable.Empty<ProductDto>();

    // Métricas calculadas para las tarjetas KPI
    public decimal TotalGlobalValue => InventorySummary.Sum(x => x.TotalInventoryValue);
    public int TotalGlobalUnits => InventorySummary.Sum(x => x.TotalUnits);
    public int TotalGlobalProducts => InventorySummary.Sum(x => x.ProductCount);

    /// <summary>
    /// Carga en paralelo las consultas de valorización y bajo stock al renderizar la página.
    /// </summary>
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

    /// <summary>
    /// Manejador POST para registrar un nuevo producto a través de la API.
    /// </summary>
    public async Task<IActionResult> OnPostCreateProductAsync()
    {
        if (!ModelState.IsValid)
        {
            var errorList = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .Distinct()
                .ToList();

            StatusTitle = "Advertencia en el Formulario";
            StatusMessage = errorList.Count > 0
                ? $"<ul class='text-start mb-0 ps-3'><li>{string.Join("</li><li>", errorList)}</li></ul>"
                : "Por favor, verifica los campos ingresados.";
            StatusType = "warning";

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
            StatusTitle = "¡Producto Creado!";
            StatusMessage = $"El producto <strong>{NewProduct.Name}</strong> ha sido registrado exitosamente en el inventario.";
            StatusType = "success";
        }
        else
        {
            StatusTitle = "Error al Registrar Producto";
            StatusMessage = !string.IsNullOrEmpty(errorMessage)
                ? errorMessage
                : "No se pudo registrar el producto. Verifica que la API y la base de datos estén activas.";
            StatusType = "error";
        }

        return RedirectToPage();
    }
}

/// <summary>
/// Modelo de entrada y validación para el formulario de creación de productos.
/// Incluye DataAnnotations y validación de expresiones regulares contra inyecciones SQL.
/// </summary>
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

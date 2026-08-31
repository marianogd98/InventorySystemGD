// Inventory.Web/Pages/Index.cshtml.cs
using System.ComponentModel.DataAnnotations;
using Inventory.Domain.Models;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventory.Web.Pages;

/// <summary>
/// Modelo de página Razor (PageModel) para el panel principal de inventario.
/// Protegido con autenticación obligatoria.
/// Gestiona la visualización de métricas analíticas, tablas y procesamiento del formulario de creación.
/// </summary>
[Authorize]
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

    // Filtro dinámico de umbral de bajo stock
    [BindProperty(SupportsGet = true)]
    public int Threshold { get; set; } = 10;

    // Parámetros de paginación para ambas tablas
    [BindProperty(SupportsGet = true)]
    public int CatPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int StockPage { get; set; } = 1;

    // Parámetros de ordenamiento (ASC / DESC) para ambas tablas
    [BindProperty(SupportsGet = true)]
    public string CatSort { get; set; } = "Category";

    [BindProperty(SupportsGet = true)]
    public string CatDir { get; set; } = "asc";

    [BindProperty(SupportsGet = true)]
    public string StockSort { get; set; } = "Stock";

    [BindProperty(SupportsGet = true)]
    public string StockDir { get; set; } = "asc";

    // Parámetros de búsqueda global para cada tabla
    [BindProperty(SupportsGet = true)]
    public string? CatSearch { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StockSearch { get; set; }

    public const int PageSize = 10;

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

    // Filtrado, Ordenamiento y Paginación para Resumen por Categoría
    public IEnumerable<CategoryInventoryValue> FilteredCategorySummary
    {
        get
        {
            var items = InventorySummary;
            if (!string.IsNullOrWhiteSpace(CatSearch))
            {
                var term = CatSearch.Trim();
                items = items.Where(x =>
                    x.Category.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.TotalUnits.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.ProductCount.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                );
            }
            return items;
        }
    }

    public int CatTotalItems => FilteredCategorySummary.Count();
    public int CatTotalPages => Math.Max(1, (int)Math.Ceiling((double)CatTotalItems / PageSize));

    public IEnumerable<CategoryInventoryValue> SortedCategorySummary =>
        (CatSort?.ToLower(), CatDir?.ToLower()) switch
        {
            ("category", "desc") => FilteredCategorySummary.OrderByDescending(x => x.Category),
            ("category", _) => FilteredCategorySummary.OrderBy(x => x.Category),
            ("productcount", "desc") => FilteredCategorySummary.OrderByDescending(x => x.ProductCount),
            ("productcount", _) => FilteredCategorySummary.OrderBy(x => x.ProductCount),
            ("totalunits", "desc") => FilteredCategorySummary.OrderByDescending(x => x.TotalUnits),
            ("totalunits", _) => FilteredCategorySummary.OrderBy(x => x.TotalUnits),
            ("totalinventoryvalue", "desc") => FilteredCategorySummary.OrderByDescending(x => x.TotalInventoryValue),
            ("totalinventoryvalue", _) => FilteredCategorySummary.OrderBy(x => x.TotalInventoryValue),
            _ => FilteredCategorySummary.OrderBy(x => x.Category)
        };

    public IEnumerable<CategoryInventoryValue> PagedCategorySummary =>
        SortedCategorySummary.Skip((Math.Max(1, CatPage) - 1) * PageSize).Take(PageSize);

    // Filtrado, Ordenamiento y Paginación para Alertas de Bajo Stock
    public IEnumerable<ProductDto> FilteredLowStockProducts
    {
        get
        {
            var items = LowStockProducts;
            if (!string.IsNullOrWhiteSpace(StockSearch))
            {
                var term = StockSearch.Trim();
                items = items.Where(x =>
                    x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Category.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Stock.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Price.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                );
            }
            return items;
        }
    }

    public int StockTotalItems => FilteredLowStockProducts.Count();
    public int StockTotalPages => Math.Max(1, (int)Math.Ceiling((double)StockTotalItems / PageSize));

    public IEnumerable<ProductDto> SortedLowStockProducts =>
        (StockSort?.ToLower(), StockDir?.ToLower()) switch
        {
            ("name", "desc") => FilteredLowStockProducts.OrderByDescending(x => x.Name),
            ("name", _) => FilteredLowStockProducts.OrderBy(x => x.Name),
            ("category", "desc") => FilteredLowStockProducts.OrderByDescending(x => x.Category),
            ("category", _) => FilteredLowStockProducts.OrderBy(x => x.Category),
            ("price", "desc") => FilteredLowStockProducts.OrderByDescending(x => x.Price),
            ("price", _) => FilteredLowStockProducts.OrderBy(x => x.Price),
            ("stock", "desc") => FilteredLowStockProducts.OrderByDescending(x => x.Stock),
            ("stock", _) => FilteredLowStockProducts.OrderBy(x => x.Stock),
            ("createdat", "desc") => FilteredLowStockProducts.OrderByDescending(x => x.CreatedAt),
            ("createdat", _) => FilteredLowStockProducts.OrderBy(x => x.CreatedAt),
            _ => FilteredLowStockProducts.OrderBy(x => x.Stock)
        };

    public IEnumerable<ProductDto> PagedLowStockProducts =>
        SortedLowStockProducts.Skip((Math.Max(1, StockPage) - 1) * PageSize).Take(PageSize);

    /// <summary>
    /// Retorna la siguiente dirección de ordenamiento ('asc' o 'desc') al hacer clic en una columna.
    /// </summary>
    public string GetCatNextDir(string column) =>
        string.Equals(CatSort, column, StringComparison.OrdinalIgnoreCase) && string.Equals(CatDir, "asc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";

    public string GetStockNextDir(string column) =>
        string.Equals(StockSort, column, StringComparison.OrdinalIgnoreCase) && string.Equals(StockDir, "asc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";

    /// <summary>
    /// Carga en paralelo las consultas de valorización y bajo stock con el umbral especificado.
    /// </summary>
    public async Task OnGetAsync()
    {
        if (Threshold < 0) Threshold = 10;
        if (CatPage < 1) CatPage = 1;
        if (StockPage < 1) StockPage = 1;

        try
        {
            var summaryTask = _apiService.GetInventoryValueByCategoryAsync();
            var lowStockTask = _apiService.GetLowStockProductsAsync(threshold: Threshold);

            await Task.WhenAll(summaryTask, lowStockTask);

            InventorySummary = await summaryTask;
            LowStockProducts = await lowStockTask;

            if (CatPage > CatTotalPages) CatPage = CatTotalPages;
            if (StockPage > StockTotalPages) StockPage = StockTotalPages;
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

        return RedirectToPage(new { 
            threshold = Threshold, 
            catPage = CatPage, 
            stockPage = StockPage,
            catSort = CatSort,
            catDir = CatDir,
            stockSort = StockSort,
            stockDir = StockDir,
            catSearch = CatSearch,
            stockSearch = StockSearch
        });
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

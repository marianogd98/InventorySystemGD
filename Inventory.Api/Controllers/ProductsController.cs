// Inventory.Api/Controllers/ProductsController.cs
using Inventory.Application.Products.Commands.AddProduct;
using Inventory.Application.Products.Queries.GetInventoryValueByCategory;
using Inventory.Application.Products.Queries.GetLowStockProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

/// <summary>
/// Controlador REST para operaciones de inventario.
/// Protegido mediante autenticación JWT (Bearer Token).
/// Desacopla la capa HTTP de la lógica de negocio delegando comandos y consultas a MediatR.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// POST api/products - Crea un nuevo producto (Command Side / EF Core).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProduct([FromBody] AddProductCommand command)
    {
        try
        {
            var productId = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, new { Id = productId });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Errores de validación de rangos numéricos del dominio (precio o stock negativo)
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Errores de invariantes de texto del dominio (nombres vacíos o caracteres de inyección SQL)
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET api/products/low-stock - Consulta productos bajo el umbral de existencias (Query Side / Dapper).
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        var query = new GetLowStockProductsQuery(threshold);
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// GET api/products/inventory-value-by-category - Reporte agregado por categoría vía Stored Procedure.
    /// </summary>
    [HttpGet("inventory-value-by-category")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryValueByCategory()
    {
        var query = new GetInventoryValueByCategoryQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}


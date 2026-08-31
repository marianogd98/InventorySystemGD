// Inventory.Api/Controllers/ProductsController.cs
using Inventory.Application.Products.Commands.AddProduct;
using Inventory.Application.Products.Queries.GetInventoryValueByCategory;
using Inventory.Application.Products.Queries.GetLowStockProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

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
    /// Crea un nuevo producto en el inventario.
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
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta los productos con stock menor o igual al umbral especificado.
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
    /// Obtiene el valor total y unidades del inventario agrupados por categoría.
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


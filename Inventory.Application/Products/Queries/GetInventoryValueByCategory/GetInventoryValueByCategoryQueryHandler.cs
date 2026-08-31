// Inventory.Application/Products/Queries/GetInventoryValueByCategory/GetInventoryValueByCategoryQueryHandler.cs
using Inventory.Domain.Interfaces;
using Inventory.Domain.Models;
using MediatR;

namespace Inventory.Application.Products.Queries.GetInventoryValueByCategory;

/// <summary>
/// Manejador de consulta para la valorización por categoría.
/// Invoca el procedimiento almacenado en SQL Server mediante Dapper.
/// </summary>
public class GetInventoryValueByCategoryQueryHandler : IRequestHandler<GetInventoryValueByCategoryQuery, IEnumerable<CategoryInventoryValue>>
{
    private readonly IInventoryQueries _inventoryQueries;

    public GetInventoryValueByCategoryQueryHandler(IInventoryQueries inventoryQueries)
    {
        _inventoryQueries = inventoryQueries ?? throw new ArgumentNullException(nameof(inventoryQueries));
    }

    public async Task<IEnumerable<CategoryInventoryValue>> Handle(
        GetInventoryValueByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _inventoryQueries.GetInventoryValueByCategoryAsync();
    }
}


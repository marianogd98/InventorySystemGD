// Inventory.Application/Products/Queries/GetLowStockProducts/GetLowStockProductsQueryHandler.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using MediatR;

namespace Inventory.Application.Products.Queries.GetLowStockProducts;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, IEnumerable<Product>>
{
    private readonly IInventoryQueries _inventoryQueries;

    public GetLowStockProductsQueryHandler(IInventoryQueries inventoryQueries)
    {
        _inventoryQueries = inventoryQueries ?? throw new ArgumentNullException(nameof(inventoryQueries));
    }

    public async Task<IEnumerable<Product>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Threshold < 0)
            throw new ArgumentOutOfRangeException(nameof(request.Threshold), "El umbral de stock no puede ser negativo.");

        return await _inventoryQueries.GetLowStockProductsAsync(request.Threshold);
    }
}


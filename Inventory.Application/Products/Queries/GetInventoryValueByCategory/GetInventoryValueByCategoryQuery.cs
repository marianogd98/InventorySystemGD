// Inventory.Application/Products/Queries/GetInventoryValueByCategory/GetInventoryValueByCategoryQuery.cs
using Inventory.Domain.Models;
using MediatR;

namespace Inventory.Application.Products.Queries.GetInventoryValueByCategory;

/// <summary>
/// Consulta CQRS para obtener el reporte consolidado de valorización y existencias por categoría.
/// </summary>
public record GetInventoryValueByCategoryQuery() : IRequest<IEnumerable<CategoryInventoryValue>>;


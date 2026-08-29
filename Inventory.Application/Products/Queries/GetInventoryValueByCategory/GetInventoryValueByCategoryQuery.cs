// Inventory.Application/Products/Queries/GetInventoryValueByCategory/GetInventoryValueByCategoryQuery.cs
using Inventory.Domain.Models;
using MediatR;

namespace Inventory.Application.Products.Queries.GetInventoryValueByCategory;

public record GetInventoryValueByCategoryQuery() : IRequest<IEnumerable<CategoryInventoryValue>>;


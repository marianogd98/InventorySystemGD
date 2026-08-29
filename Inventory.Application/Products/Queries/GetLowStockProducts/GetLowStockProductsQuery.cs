// Inventory.Application/Products/Queries/GetLowStockProducts/GetLowStockProductsQuery.cs
using Inventory.Domain.Entities;
using MediatR;

namespace Inventory.Application.Products.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery(int Threshold) : IRequest<IEnumerable<Product>>;


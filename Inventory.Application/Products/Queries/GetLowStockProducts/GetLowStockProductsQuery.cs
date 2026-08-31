// Inventory.Application/Products/Queries/GetLowStockProducts/GetLowStockProductsQuery.cs
using Inventory.Domain.Entities;
using MediatR;

namespace Inventory.Application.Products.Queries.GetLowStockProducts;

/// <summary>
/// Consulta CQRS para obtener productos con stock crítico (menor o igual al umbral).
/// </summary>
public record GetLowStockProductsQuery(int Threshold) : IRequest<IEnumerable<Product>>;


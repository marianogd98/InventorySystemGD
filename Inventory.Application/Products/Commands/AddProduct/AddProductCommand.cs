// Inventory.Application/Products/Commands/AddProduct/AddProductCommand.cs
using MediatR;

namespace Inventory.Application.Products.Commands.AddProduct;

/// <summary>
/// Comando CQRS para la creación de un nuevo producto.
/// Retorna el identificador único (Guid) generado para la entidad.
/// </summary>
public record AddProductCommand(
    string Name,
    string Category,
    decimal Price,
    int Stock
) : IRequest<Guid>;


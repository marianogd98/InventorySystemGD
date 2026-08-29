// Inventory.Application/Products/Commands/AddProduct/AddProductCommand.cs
using MediatR;

namespace Inventory.Application.Products.Commands.AddProduct;

public record AddProductCommand(
    string Name,
    string Category,
    decimal Price,
    int Stock
) : IRequest<Guid>;


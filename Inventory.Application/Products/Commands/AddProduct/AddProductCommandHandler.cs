// Inventory.Application/Products/Commands/AddProduct/AddProductCommandHandler.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Products.Commands.AddProduct;

/// <summary>
/// Manejador del comando AddProductCommand.
/// Orquesta la instanciación con el Dominio y persiste los cambios mediante EF Core.
/// </summary>
public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AddProductCommandHandler> _logger;

    public AddProductCommandHandler(
        IProductRepository productRepository,
        ILogger<AddProductCommandHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de producto: '{ProductName}' en la categoría '{Category}'", request.Name, request.Category);

        // 1. Creación segura y validación mediante Factory Method del Dominio (DDD)
        var product = Product.Create(
            name: request.Name,
            category: request.Category,
            price: request.Price,
            stock: request.Stock
        );

        // 2. Persistencia en base de datos a través del repositorio EF Core
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Producto creado exitosamente. Id: {ProductId}, Nombre: '{ProductName}'", product.Id, product.Name);

        return product.Id;
    }
}

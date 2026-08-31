// Inventory.Application/Products/Commands/AddProduct/AddProductCommandHandler.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Products.Commands.AddProduct;

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
        _logger.LogInformation("Iniciando creación de producto con Nombre: {ProductName}, Categoría: {Category}", request.Name, request.Category);

        // Se valida e instancia la entidad a través del Factory Method del Dominio
        var product = Product.Create(
            name: request.Name,
            category: request.Category,
            price: request.Price,
            stock: request.Stock
        );

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Producto creado exitosamente con Id: {ProductId}, Nombre: {ProductName}", product.Id, product.Name);

        return product.Id;
    }
}

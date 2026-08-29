// Inventory.Application/Products/Commands/AddProduct/AddProductCommandHandler.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using MediatR;

namespace Inventory.Application.Products.Commands.AddProduct;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;

    public AddProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        // Se valida e instancia la entidad a través del Factory Method del Dominio
        var product = Product.Create(
            name: request.Name,
            category: request.Category,
            price: request.Price,
            stock: request.Stock
        );

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return product.Id;
    }
}


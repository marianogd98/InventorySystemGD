// Inventory.Infrastructure/Repositories/ProductRepository.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        await _context.Products.AddAsync(product);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


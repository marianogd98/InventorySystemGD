// Inventory.Infrastructure/Repositories/ProductRepository.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta del repositorio de productos utilizando Entity Framework Core.
/// Encapsula las operaciones de inserción y confirmación transaccional.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Agrega una entidad al ChangeTracker de EF Core.
    /// </summary>
    public async Task AddAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        await _context.Products.AddAsync(product);
    }

    /// <summary>
    /// Ejecuta el INSERT SQL y confirma los cambios en la base de datos de manera atómica.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


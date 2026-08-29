// Inventory.Domain/Interfaces/IProductRepository.cs
using Inventory.Domain.Entities;

namespace Inventory.Domain.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task SaveChangesAsync();
}


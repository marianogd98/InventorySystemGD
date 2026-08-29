// Inventory.Domain/Interfaces/IInventoryQueries.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Models;

namespace Inventory.Domain.Interfaces;

public interface IInventoryQueries
{
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync();
}

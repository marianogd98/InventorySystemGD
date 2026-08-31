// Inventory.Domain/Interfaces/IProductRepository.cs
using Inventory.Domain.Entities;

namespace Inventory.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para el agregado Product (Command Side / Escritura).
/// Sigue el principio de inversión de dependencias (DIP).
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Registra una nueva entidad Product en el contexto de persistencia.
    /// </summary>
    Task AddAsync(Product product);

    /// <summary>
    /// Confirma las transacciones pendientes en la base de datos de manera asíncrona.
    /// </summary>
    Task SaveChangesAsync();
}


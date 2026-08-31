// Inventory.Domain/Interfaces/IInventoryQueries.cs
using Inventory.Domain.Entities;
using Inventory.Domain.Models;

namespace Inventory.Domain.Interfaces;

/// <summary>
/// Contrato de consultas de solo lectura (Query Side / Dapper).
/// Optimizado para consultas de alto rendimiento y ejecución de Stored Procedures.
/// </summary>
public interface IInventoryQueries
{
    /// <summary>
    /// Consulta directa con Dapper para listar productos con existencias menores o iguales al umbral.
    /// </summary>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);

    /// <summary>
    /// Ejecuta el Stored Procedure 'sp_GetInventoryValueByCategory' para obtener la valorización consolidada.
    /// </summary>
    Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync();
}

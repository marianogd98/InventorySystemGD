// Inventory.Domain/Models/CategoryInventoryValue.cs
namespace Inventory.Domain.Models;

/// <summary>
/// Modelo de proyección inmutable para consultas analíticas de inventario (Query Side / Dapper).
/// Representa el resultado consolidado por categoría obtenido mediante Stored Procedure.
/// </summary>
public record CategoryInventoryValue
{
    public string Category { get; init; } = string.Empty;
    public decimal TotalInventoryValue { get; init; }
    public int TotalUnits { get; init; }
    public int ProductCount { get; init; }
}


// Inventory.Domain/Models/CategoryInventoryValue.cs
namespace Inventory.Domain.Models;

public record CategoryInventoryValue
{
    public string Category { get; init; } = string.Empty;
    public decimal TotalInventoryValue { get; init; }
    public int TotalUnits { get; init; }
    public int ProductCount { get; init; }
}


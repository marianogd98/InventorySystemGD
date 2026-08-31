// Inventory.Domain/Entities/Product.cs
using System.Text.RegularExpressions;

namespace Inventory.Domain.Entities;

public class Product
{
    /// <summary>
    /// Expresión regular para admitir únicamente texto seguro (alfanumérico, tildes, ñ y signos no destructivos).
    /// Previene vectores comunes de inyección SQL como ', ", ;, --, /*, etc.
    /// </summary>
    private static readonly Regex SafeTextRegex = new(
        @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s.,_/#()\-]+$",
        RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor sin parámetros requerido por ORMs / Mappers (EF Core, Dapper)
    private Product() { }

    // Constructor privado para encapsular la instanciación y obligar el paso por el Factory Method
    private Product(Guid id, string name, string category, decimal price, int stock, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
        Stock = stock;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory Method: Valida invariantes del dominio y asegura que la entidad nunca nazca en un estado inválido.
    /// </summary>
    public static Product Create(string name, string category, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del producto no puede estar vacío.", nameof(name));

        if (!SafeTextRegex.IsMatch(name))
            throw new ArgumentException("El nombre del producto contiene caracteres especiales no permitidos para prevenir inyecciones SQL.", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("La categoría del producto no puede estar vacía.", nameof(category));

        if (!SafeTextRegex.IsMatch(category))
            throw new ArgumentException("La categoría del producto contiene caracteres especiales no permitidos para prevenir inyecciones SQL.", nameof(category));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "El precio no puede ser negativo.");

        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "El stock inicial no puede ser negativo.");

        return new Product(
            id: Guid.NewGuid(),
            name: name.Trim(),
            category: category.Trim(),
            price: price,
            stock: stock,
            createdAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Actualiza el precio del producto garantizando que permanezca positivo.
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), "El precio no puede ser negativo.");

        Price = newPrice;
    }

    /// <summary>
    /// Incrementa las existencias del producto validando cantidades estrictamente positivas.
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a ingresar debe ser mayor a cero.", nameof(quantity));

        Stock += quantity;
    }

    /// <summary>
    /// Reduce existencias protegiendo la regla de negocio de no permitir stock negativo.
    /// </summary>
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a retirar debe ser mayor a cero.", nameof(quantity));

        if (Stock - quantity < 0)
            throw new InvalidOperationException($"Stock insuficiente. Stock actual: {Stock}, solicitado: {quantity}.");

        Stock -= quantity;
    }
}


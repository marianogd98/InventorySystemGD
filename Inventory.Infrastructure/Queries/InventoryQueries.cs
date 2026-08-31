// Inventory.Infrastructure/Queries/InventoryQueries.cs
using System.Data;
using Dapper;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Models;
using Microsoft.Data.SqlClient;

namespace Inventory.Infrastructure.Queries;

/// <summary>
/// Implementación de consultas de alto rendimiento con Dapper (Query Side).
/// Ejecuta consultas directas SQL y Stored Procedures mapeando a modelos y entidades.
/// </summary>
public class InventoryQueries : IInventoryQueries
{
    private readonly string _connectionString;

    public InventoryQueries(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <summary>
    /// Consulta directa SQL con Dapper para listar productos con stock bajo.
    /// Utiliza el hint WITH (NOLOCK) para evitar bloqueos en operaciones de solo lectura de inventario.
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        const string sql = @"
            SELECT 
                Id, 
                Name, 
                Category, 
                Price, 
                Stock, 
                CreatedAt
            FROM dbo.Products WITH (NOLOCK)
            WHERE Stock <= @Threshold
            ORDER BY Stock ASC;";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<Product>(sql, new { Threshold = threshold });
    }

    /// <summary>
    /// Invoca el procedimiento almacenado 'sp_GetInventoryValueByCategory' de SQL Server.
    /// Calcula de forma agregada el valor monetario y conteo de productos por categoría.
    /// </summary>
    public async Task<IEnumerable<CategoryInventoryValue>> GetInventoryValueByCategoryAsync()
    {
        const string storedProcedure = "dbo.sp_GetInventoryValueByCategory";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<CategoryInventoryValue>(
            storedProcedure,
            commandType: CommandType.StoredProcedure
        );
    }
}

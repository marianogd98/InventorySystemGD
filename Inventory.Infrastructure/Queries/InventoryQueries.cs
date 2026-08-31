// Inventory.Infrastructure/Queries/InventoryQueries.cs
using System.Data;
using Dapper;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Models;
using Microsoft.Data.SqlClient;

namespace Inventory.Infrastructure.Queries;

public class InventoryQueries : IInventoryQueries
{
    private readonly string _connectionString;

    public InventoryQueries(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

        _connectionString = connectionString;
    }

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

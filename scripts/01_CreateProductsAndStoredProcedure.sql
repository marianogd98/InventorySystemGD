-- scripts/01_CreateProductsAndStoredProcedure.sql

-- ==========================================================
-- 0. Creación de la Base de Datos InventoryDb si no existe
-- ==========================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryDb')
BEGIN
    CREATE DATABASE InventoryDb;
END
GO

USE InventoryDb;
GO

-- ==========================================================
-- 1. Creación de la Tabla: Products
-- ==========================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Products (
        Id UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        Stock INT NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL,
        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_Products_Category ON dbo.Products(Category) INCLUDE (Price, Stock);
    CREATE NONCLUSTERED INDEX IX_Products_Stock ON dbo.Products(Stock);
END
GO

-- ==========================================================
-- 2. Procedimiento Almacenado: sp_GetInventoryValueByCategory
-- ==========================================================
IF OBJECT_ID('dbo.sp_GetInventoryValueByCategory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetInventoryValueByCategory;
GO

CREATE PROCEDURE dbo.sp_GetInventoryValueByCategory
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Category,
        SUM(CAST(Stock AS DECIMAL(18, 2)) * Price) AS TotalInventoryValue,
        SUM(Stock) AS TotalUnits,
        COUNT(1) AS ProductCount
    FROM 
        dbo.Products WITH (NOLOCK)
    GROUP BY 
        Category
    ORDER BY 
        TotalInventoryValue DESC;
END
GO

// Inventory.UnitTests/Commands/AddProductCommandHandlerTests.cs
using Inventory.Application.Products.Commands.AddProduct;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Inventory.UnitTests.Commands;

public class AddProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<AddProductCommandHandler>> _loggerMock;
    private readonly AddProductCommandHandler _handler;

    public AddProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<AddProductCommandHandler>>();
        _handler = new AddProductCommandHandler(_productRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductSaveAndReturnNonEmptyGuid()
    {
        // Arrange (Dado)
        var command = new AddProductCommand(
            Name: "Teclado Mecánico RGB",
            Category: "Periféricos",
            Price: 89.99m,
            Stock: 15
        );

        _productRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act (Cuando)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Entonces)
        Assert.NotEqual(Guid.Empty, result);

        _productRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<Product>(p =>
                p.Name == command.Name &&
                p.Category == command.Category &&
                p.Price == command.Price &&
                p.Stock == command.Stock
            )),
            Times.Once
        );

        _productRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(),
            Times.Once
        );
    }

    [Theory]
    [InlineData("", "Electrónica", 100, 10)]
    [InlineData("   ", "Electrónica", 100, 10)]
    [InlineData("Mouse", "", 100, 10)]
    [InlineData("Mouse", "   ", 100, 10)]
    public async Task Handle_WithInvalidNameOrCategory_ShouldThrowArgumentException(
        string name,
        string category,
        decimal price,
        int stock)
    {
        // Arrange
        var command = new AddProductCommand(name, category, price, stock);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativePrice_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var command = new AddProductCommand("Monitor 4K", "Pantallas", -250.00m, 5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeStock_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var command = new AddProductCommand("Monitor 4K", "Pantallas", 250.00m, -5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}


using CarSales.Application.DTOs;
using CarSales.Application.Interfaces;
using CarSales.Application.Services;
using CarSales.Domain.Entities;
using CarSales.Domain.Enums;
using Moq;

namespace CarSales.Tests;

public class SaleServiceTests
{
    [Fact]
    public async Task GetTotalSalesAsync_NoSales_ReturnsZeroTotals()
    {
        var service = CreateService(out var repository);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        var total = await service.GetTotalSalesAsync();

        Assert.Equal(0, total.TotalUnits);
        Assert.Equal(0m, total.TotalAmount);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTotalSalesAsync_OneSale_ReturnsSaleTotals()
    {
        var service = CreateService(out var repository);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(
            new[] { CreateSale(CarModel.SUV, DistributionCenter.Center1, 2, 19000m) });

        var total = await service.GetTotalSalesAsync();

        Assert.Equal(2, total.TotalUnits);
        Assert.Equal(19000m, total.TotalAmount);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTotalSalesAsync_MultipleSales_ReturnsAggregatedTotals()
    {
        var service = CreateService(out var repository);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(
            new[]
            {
                CreateSale(CarModel.Sedan, DistributionCenter.Center1, 2, 16000m),
                CreateSale(CarModel.Sport, DistributionCenter.Center2, 3, 58422m),
                CreateSale(CarModel.Offroad, DistributionCenter.Center3, 1, 12500m)
            });

        var total = await service.GetTotalSalesAsync();

        Assert.Equal(6, total.TotalUnits);
        Assert.Equal(86922m, total.TotalAmount);
        repository.Verify(mock => mock.GetAllAsync(), Times.Once);
    }

    [Theory]
    [InlineData(CarModel.Sedan, 8000)]
    [InlineData(CarModel.SUV, 9500)]
    [InlineData(CarModel.Offroad, 12500)]
    [InlineData(CarModel.Sport, 19474)]
    public async Task CreateSaleAsync_Model_ReturnsExpectedUnitPriceAndTotal(
        CarModel model,
        decimal expectedPrice)
    {
        var service = CreateService(out _);

        var sale = await service.CreateSaleAsync(CreateRequest(model));

        Assert.Equal(expectedPrice, sale.UnitPrice);
        Assert.Equal(expectedPrice, sale.TotalAmount);
    }

    [Fact]
    public async Task CreateSaleAsync_SedanWithQuantityThree_CalculatesTotalAmount()
    {
        var service = CreateService(out _);

        var sale = await service.CreateSaleAsync(CreateRequest(CarModel.Sedan, 3));

        Assert.Equal(8000m, sale.UnitPrice);
        Assert.Equal(24000m, sale.TotalAmount);
    }

    [Fact]
    public async Task CreateSaleAsync_SportWithQuantityTen_CalculatesTotalAmount()
    {
        var service = CreateService(out _);

        var sale = await service.CreateSaleAsync(CreateRequest(CarModel.Sport, 10));

        Assert.Equal(19474m, sale.UnitPrice);
        Assert.Equal(194740m, sale.TotalAmount);
    }

    [Fact]
    public async Task CreateSaleAsync_ValidSale_PersistsSaleWithCalculatedValues()
    {
        var service = CreateService(out var repository);

        await service.CreateSaleAsync(CreateRequest(CarModel.Sport, 3));

        repository.Verify(
            mock => mock.AddAsync(It.Is<Sale>(sale =>
                sale.Model == CarModel.Sport &&
                sale.DistributionCenter == DistributionCenter.Center2 &&
                sale.Quantity == 3 &&
                sale.UnitPrice == 19474m &&
                sale.TotalAmount == 58422m)),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSaleAsync_InvalidQuantity_ThrowsExceptionAndDoesNotPersist(int quantity)
    {
        var service = CreateService(out var repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.CreateSaleAsync(CreateRequest(CarModel.Sedan, quantity)));

        repository.Verify(mock => mock.AddAsync(It.IsAny<Sale>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaleAsync_ValidSale_SetsAllSaleData()
    {
        var service = CreateService(out _);
        var beforeCreation = DateTime.UtcNow;

        var sale = await service.CreateSaleAsync(
            CreateRequest(CarModel.Offroad, 2, DistributionCenter.Center3));

        var afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(CarModel.Offroad, sale.Model);
        Assert.Equal(DistributionCenter.Center3, sale.DistributionCenter);
        Assert.Equal(2, sale.Quantity);
        Assert.Equal(12500m, sale.UnitPrice);
        Assert.Equal(25000m, sale.TotalAmount);
        Assert.InRange(sale.CreatedAt, beforeCreation, afterCreation);
    }

    private static SaleService CreateService(out Mock<ISaleRepository> repository)
    {
        repository = new Mock<ISaleRepository>();
        repository.Setup(mock => mock.AddAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);

        return new SaleService(repository.Object);
    }

    private static CreateSaleRequest CreateRequest(
        CarModel model,
        int quantity = 1,
        DistributionCenter distributionCenter = DistributionCenter.Center2)
    {
        return new CreateSaleRequest
        {
            Model = model,
            DistributionCenter = distributionCenter,
            Quantity = quantity
        };
    }

    private static Sale CreateSale(
        CarModel model,
        DistributionCenter distributionCenter,
        int quantity,
        decimal totalAmount)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            Model = model,
            DistributionCenter = distributionCenter,
            Quantity = quantity,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };
    }
}

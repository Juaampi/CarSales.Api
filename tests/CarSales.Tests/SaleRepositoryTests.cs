using CarSales.Domain.Entities;
using CarSales.Domain.Enums;
using CarSales.Infrastructure.Repositories;

namespace CarSales.Tests;

public class SaleRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyCollection()
    {
        var repository = new SaleRepository();

        var sales = await repository.GetAllAsync();

        Assert.Empty(sales);
    }

    [Fact]
    public async Task AddAsync_SaleAdded_ReturnsStoredSale()
    {
        var repository = new SaleRepository();
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Model = CarModel.Sedan,
            DistributionCenter = DistributionCenter.Center1,
            Quantity = 2,
            UnitPrice = 8000m,
            TotalAmount = 16000m,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(sale);
        var sales = await repository.GetAllAsync();

        var storedSale = Assert.Single(sales);
        Assert.Same(sale, storedSale);
    }
}

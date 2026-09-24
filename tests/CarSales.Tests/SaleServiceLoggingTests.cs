using CarSales.Application.DTOs;
using CarSales.Application.Interfaces;
using CarSales.Application.Services;
using CarSales.Domain.Entities;
using CarSales.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarSales.Tests;

public class SaleServiceLoggingTests
{
    [Fact]
    public async Task CreateSaleAsync_ValidSale_LogsInformation()
    {
        var service = CreateService(out _, out var logger);

        await service.CreateSaleAsync(new CreateSaleRequest
        {
            Model = CarModel.Sedan,
            DistributionCenter = DistributionCenter.Center1,
            Quantity = 1
        });

        VerifyInformationLog(logger, "Venta creada");
    }

    [Fact]
    public async Task GetTotalSalesAsync_Executed_LogsInformation()
    {
        var service = CreateService(out var repository, out var logger);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        await service.GetTotalSalesAsync();

        VerifyInformationLog(logger, "GetTotalSalesAsync");
    }

    [Fact]
    public async Task GetSalesByCenterAsync_Executed_LogsInformation()
    {
        var service = CreateService(out var repository, out var logger);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        await service.GetSalesByCenterAsync();

        VerifyInformationLog(logger, "GetSalesByCenterAsync");
    }

    [Fact]
    public async Task GetSalesPercentageByModelAsync_Executed_LogsInformation()
    {
        var service = CreateService(out var repository, out var logger);
        repository.Setup(mock => mock.GetAllAsync()).ReturnsAsync(Array.Empty<Sale>());

        await service.GetSalesPercentageByModelAsync();

        VerifyInformationLog(logger, "GetSalesPercentageByModelAsync");
    }

    private static SaleService CreateService(
        out Mock<ISaleRepository> repository,
        out Mock<ILogger<SaleService>> logger)
    {
        repository = new Mock<ISaleRepository>();
        logger = new Mock<ILogger<SaleService>>();

        return new SaleService(repository.Object, logger.Object);
    }

    private static void VerifyInformationLog(
        Mock<ILogger<SaleService>> logger,
        string operation)
    {
        logger.Verify(
            mock => mock.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(operation)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

using CarSales.Api.Controllers;
using CarSales.Api.DTOs;
using CarSales.Application.DTOs;
using CarSales.Application.Errors;
using CarSales.Application.Interfaces;
using CarSales.Domain.Entities;
using CarSales.Domain.Enums;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarSales.Tests;

public class SaleControllerTests
{
    [Fact]
    public async Task CreateSaleAsync_SuccessfulResult_ReturnsCreatedResponse()
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Model = CarModel.Sport,
            DistributionCenter = DistributionCenter.Center2,
            Quantity = 3,
            UnitPrice = 19474m,
            TotalAmount = 58422m,
            CreatedAt = DateTime.UtcNow
        };
        var service = new Mock<ISaleService>();
        service.Setup(mock => mock.CreateSaleAsync(It.IsAny<CreateSaleRequest>()))
            .ReturnsAsync(Result.Success<Sale, Error>(sale));
        var controller = new SaleController(service.Object);

        var actionResult = await controller.CreateSaleAsync(new CreateSaleRequest());

        var response = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.IsType<SaleResponse>(response.Value);
    }

    [Fact]
    public async Task CreateSaleAsync_InvalidQuantity_ReturnsBadRequestWithErrorResponse()
    {
        var service = new Mock<ISaleService>();
        service.Setup(mock => mock.CreateSaleAsync(It.IsAny<CreateSaleRequest>()))
            .ReturnsAsync(Result.Failure<Sale, Error>(Errors.InvalidQuantity));
        var controller = new SaleController(service.Object);

        var actionResult = await controller.CreateSaleAsync(new CreateSaleRequest());

        var response = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var error = Assert.IsType<ErrorResponse>(response.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal("Sale.InvalidQuantity", error.Code);
        Assert.Equal("La cantidad debe ser mayor a cero.", error.Message);
    }
}

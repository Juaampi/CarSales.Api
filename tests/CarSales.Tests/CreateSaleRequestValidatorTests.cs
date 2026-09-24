using CarSales.Application.DTOs;
using CarSales.Application.Validators;
using CarSales.Domain.Enums;

namespace CarSales.Tests;

public class CreateSaleRequestValidatorTests
{
    private readonly CreateSaleRequestValidator validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = validator.Validate(new CreateSaleRequest
        {
            Model = CarModel.Sedan,
            DistributionCenter = DistributionCenter.Center1,
            Quantity = 1
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingRequiredFields_ReturnsErrors()
    {
        var result = validator.Validate(new CreateSaleRequest());

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSaleRequest.Model));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateSaleRequest.DistributionCenter));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSaleRequest.Quantity));
    }

    [Fact]
    public void Validate_InvalidEnums_ReturnsErrors()
    {
        var result = validator.Validate(new CreateSaleRequest
        {
            Model = (CarModel)999,
            DistributionCenter = (DistributionCenter)999,
            Quantity = 1
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSaleRequest.Model));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateSaleRequest.DistributionCenter));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveQuantity_ReturnsError(int quantity)
    {
        var result = validator.Validate(new CreateSaleRequest
        {
            Model = CarModel.Sedan,
            DistributionCenter = DistributionCenter.Center1,
            Quantity = quantity
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSaleRequest.Quantity));
    }
}

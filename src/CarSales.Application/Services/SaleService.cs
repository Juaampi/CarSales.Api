using CarSales.Application.DTOs;
using CarSales.Application.Interfaces;
using CarSales.Domain.Entities;
using CarSales.Domain.Enums;

namespace CarSales.Application.Services;

public class SaleService(ISaleRepository saleRepository) : ISaleService
{
    public async Task<Sale> CreateSaleAsync(CreateSaleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        var unitPrice = GetUnitPrice(request.Model);
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Model = request.Model,
            DistributionCenter = request.DistributionCenter,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            TotalAmount = unitPrice * request.Quantity,
            CreatedAt = DateTime.UtcNow
        };

        await saleRepository.AddAsync(sale);

        return sale;
    }

    public async Task<SalesTotalResponse> GetTotalSalesAsync()
    {
        var sales = await saleRepository.GetAllAsync();

        return new SalesTotalResponse(
            sales.Sum(sale => sale.Quantity),
            sales.Sum(sale => sale.TotalAmount));
    }

    public async Task<IReadOnlyCollection<SalesByCenterResponse>> GetSalesByCenterAsync()
    {
        var sales = await saleRepository.GetAllAsync();
        var totalsByCenter = sales
            .GroupBy(sale => sale.DistributionCenter)
            .ToDictionary(
                group => group.Key,
                group => new SalesByCenterResponse(
                    group.Key,
                    group.Sum(sale => sale.Quantity),
                    group.Sum(sale => sale.TotalAmount)));

        return Enum.GetValues<DistributionCenter>()
            .Select(center => totalsByCenter.TryGetValue(
                center,
                out var total)
                ? total
                : new SalesByCenterResponse(center, 0, 0m))
            .ToArray();
    }

    private static void ValidateRequest(CreateSaleRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Quantity), "Quantity must be greater than zero.");
        }

        if (!Enum.IsDefined(request.Model))
        {
            throw new ArgumentOutOfRangeException(nameof(request.Model), "The car model is invalid.");
        }

        if (!Enum.IsDefined(request.DistributionCenter))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.DistributionCenter),
                "The distribution center is invalid.");
        }
    }

    private static decimal GetUnitPrice(CarModel model)
    {
        return model switch
        {
            CarModel.Sedan => 8000m,
            CarModel.SUV => 9500m,
            CarModel.Offroad => 12500m,
            CarModel.Sport => 18200m * 1.07m,
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "The car model is invalid.")
        };
    }
}

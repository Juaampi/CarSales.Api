using CarSales.Application.DTOs;
using CarSales.Application.Errors;
using CarSales.Application.Interfaces;
using CarSales.Domain.Entities;
using CarSales.Domain.Enums;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CarSales.Application.Services;

public class SaleService(
    ISaleRepository saleRepository,
    ILogger<SaleService> logger) : ISaleService
{
    public async Task<Result<Sale, Error>> CreateSaleAsync(CreateSaleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // La cantidad es un error esperado del negocio, por eso se devuelve como Result.
        if (request.Quantity <= 0)
        {
            return global::CarSales.Application.Errors.Errors.InvalidQuantity;
        }

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

        // El repositorio solo guarda la venta; los precios y el total se resuelven acá.
        await saleRepository.AddAsync(sale);

        logger.LogInformation(
            "Venta creada. Id: {SaleId}, Modelo: {Model}, Centro: {Center}, Unidades: {Quantity}, Total: {TotalAmount}",
            sale.Id,
            sale.Model,
            sale.DistributionCenter,
            sale.Quantity,
            sale.TotalAmount);

        return Result.Success<Sale, Error>(sale);
    }

    public async Task<SalesTotalResponse> GetTotalSalesAsync()
    {
        var sales = await saleRepository.GetAllAsync();

        // Las consultas trabajan con los importes que ya fueron calculados al crear la venta.
        var result = new SalesTotalResponse(
            sales.Sum(sale => sale.Quantity),
            sales.Sum(sale => sale.TotalAmount));

        logger.LogInformation("GetTotalSalesAsync ejecutada.");

        return result;
    }

    public async Task<IReadOnlyCollection<SalesByCenterResponse>> GetSalesByCenterAsync()
    {
        var sales = await saleRepository.GetAllAsync();

        // Primero agrupamos los centros que tienen ventas y después completamos los que están en cero.
        var totalsByCenter = sales
            .GroupBy(sale => sale.DistributionCenter)
            .ToDictionary(
                group => group.Key,
                group => new SalesByCenterResponse(
                    group.Key,
                    group.Sum(sale => sale.Quantity),
                    group.Sum(sale => sale.TotalAmount)));

        var result = Enum.GetValues<DistributionCenter>()
            .Select(center => totalsByCenter.TryGetValue(
                center,
                out var total)
                ? total
                : new SalesByCenterResponse(center, 0, 0m))
            .ToArray();

        logger.LogInformation("GetSalesByCenterAsync ejecutada.");

        return result;
    }

    public async Task<IReadOnlyCollection<SalesPercentageByModelResponse>> GetSalesPercentageByModelAsync()
    {
        var sales = await saleRepository.GetAllAsync();
        var totalUnits = sales.Sum(sale => sale.Quantity);

        // El porcentaje usa como denominador el total general de unidades, no el total del centro.
        var unitsByCombination = sales
            .GroupBy(sale => new { sale.DistributionCenter, sale.Model })
            .ToDictionary(
                group => (group.Key.DistributionCenter, group.Key.Model),
                group => group.Sum(sale => sale.Quantity));

        var result = Enum.GetValues<DistributionCenter>()
            .SelectMany(center => Enum.GetValues<CarModel>()
                .Select(model =>
                {
                    var units = unitsByCombination.GetValueOrDefault((center, model));
                    var percentage = totalUnits == 0
                        ? 0m
                        : Math.Round(units * 100m / totalUnits, 2, MidpointRounding.AwayFromZero);

                    return new SalesPercentageByModelResponse(center, model, units, percentage);
                }))
            .ToArray();

        logger.LogInformation("GetSalesPercentageByModelAsync ejecutada.");

        return result;
    }

    private static void ValidateRequest(CreateSaleRequest request)
    {
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
            // Sport es el único modelo al que se le aplica el impuesto adicional del 7%.
            CarModel.Sport => 18200m * 1.07m,
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "The car model is invalid.")
        };
    }
}

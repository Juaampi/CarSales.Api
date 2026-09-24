using CarSales.Domain.Enums;

namespace CarSales.Application.DTOs;

public sealed record SalesPercentageByModelResponse(
    DistributionCenter DistributionCenter,
    CarModel Model,
    int Units,
    decimal Percentage);

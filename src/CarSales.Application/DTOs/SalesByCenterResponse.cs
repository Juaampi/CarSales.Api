using CarSales.Domain.Enums;

namespace CarSales.Application.DTOs;

public sealed record SalesByCenterResponse(
    DistributionCenter DistributionCenter,
    int TotalUnits,
    decimal TotalAmount);

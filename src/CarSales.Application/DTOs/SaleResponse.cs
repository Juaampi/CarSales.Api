using CarSales.Domain.Entities;
using CarSales.Domain.Enums;

namespace CarSales.Application.DTOs;

public sealed record SaleResponse(
    Guid Id,
    CarModel Model,
    DistributionCenter DistributionCenter,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTimeOffset CreatedAt)
{
    public static SaleResponse FromSale(Sale sale)
    {
        return new SaleResponse(
            sale.Id,
            sale.Model,
            sale.DistributionCenter,
            sale.Quantity,
            sale.UnitPrice,
            sale.TotalAmount,
            new DateTimeOffset(sale.CreatedAt, TimeSpan.Zero));
    }
}

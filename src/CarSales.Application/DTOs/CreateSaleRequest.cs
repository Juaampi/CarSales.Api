using CarSales.Domain.Enums;

namespace CarSales.Application.DTOs;

public class CreateSaleRequest
{
    public CarModel? Model { get; set; }

    public DistributionCenter? DistributionCenter { get; set; }

    public int Quantity { get; set; }
}

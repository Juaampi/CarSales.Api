using CarSales.Domain.Enums;

namespace CarSales.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }

    public CarModel Model { get; set; }

    public DistributionCenter DistributionCenter { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

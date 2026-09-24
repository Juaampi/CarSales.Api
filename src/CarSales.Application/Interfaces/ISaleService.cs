using CarSales.Application.DTOs;
using CarSales.Domain.Entities;

namespace CarSales.Application.Interfaces;

public interface ISaleService
{
    Task<Sale> CreateSaleAsync(CreateSaleRequest request);
}

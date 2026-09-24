using CarSales.Application.DTOs;
using CarSales.Application.Errors;
using CarSales.Domain.Entities;
using CSharpFunctionalExtensions;

namespace CarSales.Application.Interfaces;

public interface ISaleService
{
    Task<Result<Sale, Error>> CreateSaleAsync(CreateSaleRequest request);

    Task<SalesTotalResponse> GetTotalSalesAsync();

    Task<IReadOnlyCollection<SalesByCenterResponse>> GetSalesByCenterAsync();

    Task<IReadOnlyCollection<SalesPercentageByModelResponse>> GetSalesPercentageByModelAsync();
}

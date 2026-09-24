using CarSales.Application.Interfaces;
using CarSales.Domain.Entities;

namespace CarSales.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly List<Sale> _sales = [];

    public Task AddAsync(Sale sale)
    {
        _sales.Add(sale);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Sale>> GetAllAsync()
    {
        IReadOnlyCollection<Sale> sales = _sales.AsReadOnly();

        return Task.FromResult(sales);
    }
}

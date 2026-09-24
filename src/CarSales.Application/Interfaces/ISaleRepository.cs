using CarSales.Domain.Entities;

namespace CarSales.Application.Interfaces;

public interface ISaleRepository
{
    Task AddAsync(Sale sale);

    Task<IReadOnlyCollection<Sale>> GetAllAsync();
}

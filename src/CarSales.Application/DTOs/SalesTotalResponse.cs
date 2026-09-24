namespace CarSales.Application.DTOs;

public sealed record SalesTotalResponse(
    int TotalUnits,
    decimal TotalAmount);

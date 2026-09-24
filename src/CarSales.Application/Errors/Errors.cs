using CSharpFunctionalExtensions;

namespace CarSales.Application.Errors;

public static class Errors
{
    public static readonly Error InvalidQuantity = new(
        "Sale.InvalidQuantity",
        "La cantidad debe ser mayor a cero.");
}

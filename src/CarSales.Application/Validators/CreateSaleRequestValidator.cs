using FluentValidation;
using CarSales.Application.DTOs;

namespace CarSales.Application.Validators;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("El modelo de coche es obligatorio.")
            .IsInEnum().WithMessage("El modelo de coche es inválido.");

        RuleFor(x => x.DistributionCenter)
            .NotNull().WithMessage("El centro de distribución es obligatorio.")
            .IsInEnum().WithMessage("El centro de distribución es inválido.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");
    }
}

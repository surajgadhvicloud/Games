using BoardGamesLibrary.Application.Contracts;
using FluentValidation;

namespace BoardGamesLibrary.Application.Validators;

public sealed class CreateInventoryRequestValidator : AbstractValidator<CreateInventoryRequest>
{
    public CreateInventoryRequestValidator()
    {
        RuleFor(x => x.BoardGameId).GreaterThan(0);
        RuleFor(x => x.TotalInventory).GreaterThan(0);
        RuleFor(x => x.AvailableInventory).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => x.AvailableInventory <= x.TotalInventory)
            .WithMessage("Available inventory cannot exceed total inventory.");
    }
}

public sealed class UpdateInventoryRequestValidator : AbstractValidator<UpdateInventoryRequest>
{
    public UpdateInventoryRequestValidator()
    {
        RuleFor(x => x.TotalInventory).GreaterThan(0);
        RuleFor(x => x.AvailableInventory).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => x.AvailableInventory <= x.TotalInventory)
            .WithMessage("Available inventory cannot exceed total inventory.");
    }
}
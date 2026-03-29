using BoardGamesLibrary.Application.Contracts;
using FluentValidation;

namespace BoardGamesLibrary.Application.Validators;

public sealed class CreateBoardGameRequestValidator : AbstractValidator<CreateBoardGameRequest>
{
    public CreateBoardGameRequestValidator()
    {
        RuleFor(x => x.GameName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .Must(UrlValidation.BeValidAbsoluteHttpUrlOrNull)
            .WithMessage("Image URL must be a valid absolute http/https URL.");
        RuleFor(x => x.MinPlayers).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxPlayers).GreaterThanOrEqualTo(x => x.MinPlayers);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateBoardGameRequestValidator : AbstractValidator<UpdateBoardGameRequest>
{
    public UpdateBoardGameRequestValidator()
    {
        RuleFor(x => x.GameName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .Must(UrlValidation.BeValidAbsoluteHttpUrlOrNull)
            .WithMessage("Image URL must be a valid absolute http/https URL.");
        RuleFor(x => x.MinPlayers).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxPlayers).GreaterThanOrEqualTo(x => x.MinPlayers);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

internal static class UrlValidation
{
    public static bool BeValidAbsoluteHttpUrlOrNull(string? url)
    {
        if (url is null)
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }
}
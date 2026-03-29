using BoardGamesLibrary.Application.Contracts;
using FluentValidation;

namespace BoardGamesLibrary.Application.Validators;

public sealed class CreateGameIssueRequestValidator : AbstractValidator<CreateGameIssueRequest>
{
    public CreateGameIssueRequestValidator()
    {
        RuleFor(x => x.BoardGameId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PhotoUrlBeforeIssue)
            .MaximumLength(2048)
            .Must(UrlValidation.BeValidAbsoluteHttpUrlOrNull)
            .WithMessage("Photo URL before issue must be a valid absolute http/https URL.");
        RuleFor(x => x.EndDateUtc)
            .GreaterThan(x => x.StartDateUtc)
            .When(x => x.StartDateUtc.HasValue && x.EndDateUtc.HasValue)
            .WithMessage("End date must be after start date.");
    }
}

public sealed class UpdateGameIssueRequestValidator : AbstractValidator<UpdateGameIssueRequest>
{
    public UpdateGameIssueRequestValidator()
    {
        RuleFor(x => x.ReturnDateUtc).NotNull().WithMessage("Return date is required for update.");
        RuleFor(x => x.ConditionGivenIn).NotNull().WithMessage("Condition on return is required for update.");
        RuleFor(x => x.PhotoUrlAfterReturn)
            .MaximumLength(2048)
            .Must(UrlValidation.BeValidAbsoluteHttpUrlOrNull)
            .WithMessage("Photo URL after return must be a valid absolute http/https URL.");
    }
}


using BoardGamesLibrary.Application.Contracts;
using FluentValidation;

namespace BoardGamesLibrary.Application.Validators;

public sealed class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PhoneNumber).NotEmpty().MinimumLength(10).MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
    }
}

public sealed class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
{
    public UpdateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PhoneNumber).NotEmpty().MinimumLength(10).MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
    }
}

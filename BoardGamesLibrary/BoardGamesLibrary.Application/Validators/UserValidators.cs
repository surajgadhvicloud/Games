using BoardGamesLibrary.Application.Contracts;
using FluentValidation;

namespace BoardGamesLibrary.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator()
	{
		RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
		RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
		RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(200);
		RuleFor(x => x.Role).IsInEnum();
	}
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
	public UpdateUserRequestValidator()
	{
		RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
		RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
		RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Password).MaximumLength(200);
		RuleFor(x => x.Role).IsInEnum();
	}
}
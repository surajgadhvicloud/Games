using BoardGamesLibrary.Domain.Enums;

namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateUserRequest(
	string FirstName,
	string LastName,
	string Email,
	string Username,
	string Password,
	UserRole Role);

public sealed record UpdateUserRequest(
	string FirstName,
	string LastName,
	string Email,
	string Username,
	string? Password,
	UserRole Role);

public sealed record UserResponse(
	int Id,
	string FirstName,
	string LastName,
	string Email,
	string Username,
	UserRole Role);
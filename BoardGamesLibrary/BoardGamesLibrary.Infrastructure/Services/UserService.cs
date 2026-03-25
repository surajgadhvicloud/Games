using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Services;

public class UserService(
	BoardGamesDbContext dbContext,
	ICurrentUserService currentUserService,
	IPasswordHasher<User> passwordHasher) : IUserService
{
	public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
	{
		var email = request.Email.Trim().ToLowerInvariant();
		var username = request.Username.Trim();

		var emailExists = await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
		if (emailExists)
		{
			throw new InvalidOperationException("User with this email already exists.");
		}

		var usernameExists = await dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken);
		if (usernameExists)
		{
			throw new InvalidOperationException("User with this username already exists.");
		}

		var entity = new User
		{
			FirstName = request.FirstName.Trim(),
			LastName = request.LastName.Trim(),
			Email = email,
			Username = username,
			Role = request.Role,
			CreatedAtUtc = DateTime.UtcNow,
			ModifiedByUser = currentUserService.GetUsername()
		};

		entity.PasswordHash = passwordHasher.HashPassword(entity, request.Password);

		dbContext.Users.Add(entity);
		await dbContext.SaveChangesAsync(cancellationToken);
		return ToResponse(entity);
	}

	public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
	{
		var entity = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
			?? throw new KeyNotFoundException($"User {id} was not found.");

		var email = request.Email.Trim().ToLowerInvariant();
		var username = request.Username.Trim();

		var emailExists = await dbContext.Users.AnyAsync(x => x.Email == email && x.Id != id, cancellationToken);
		if (emailExists)
		{
			throw new InvalidOperationException("User with this email already exists.");
		}

		var usernameExists = await dbContext.Users.AnyAsync(x => x.Username == username && x.Id != id, cancellationToken);
		if (usernameExists)
		{
			throw new InvalidOperationException("User with this username already exists.");
		}

		entity.FirstName = request.FirstName.Trim();
		entity.LastName = request.LastName.Trim();
		entity.Email = email;
		entity.Username = username;
		entity.Role = request.Role;
		entity.UpdatedAtUtc = DateTime.UtcNow;
		entity.ModifiedByUser = currentUserService.GetUsername();

		if (!string.IsNullOrWhiteSpace(request.Password))
		{
			entity.PasswordHash = passwordHasher.HashPassword(entity, request.Password);
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		return ToResponse(entity);
	}

	public async Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken cancellationToken)
	{
		return await dbContext.Users
			.OrderBy(x => x.Username)
			.Select(x => ToResponse(x))
			.ToListAsync(cancellationToken);
	}

	public async Task<UserResponse> GetAsync(int id, CancellationToken cancellationToken)
	{
		var entity = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
			?? throw new KeyNotFoundException($"User {id} was not found.");
		return ToResponse(entity);
	}

	private static UserResponse ToResponse(User entity) =>
		new(entity.Id, entity.FirstName, entity.LastName, entity.Email, entity.Username, entity.Role);
}
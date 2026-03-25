using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IUserService
{
	Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
	Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken);
	Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken cancellationToken);
	Task<UserResponse> GetAsync(int id, CancellationToken cancellationToken);
}
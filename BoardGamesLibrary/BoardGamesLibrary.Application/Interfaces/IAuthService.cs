using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(RevokeTokenRequest request, CancellationToken cancellationToken);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Configuration;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoardGamesLibrary.Infrastructure.Services;

public class AuthService(
    BoardGamesDbContext dbContext,
    IOptions<JwtOptions> jwtOptions,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var user = await dbContext.Users.FirstOrDefaultAsync(
            x => x.Username == username,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var tokenPair = await IssueTokenPairAsync(user, cancellationToken);

        return new LoginResponse(
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAtUtc,
            user.Username,
            user.Role.ToString());
    }

    public async Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var existing = await dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken)
            ?? throw new KeyNotFoundException("Refresh token was not found.");

        if (existing.RevokedAtUtc.HasValue || existing.ExpiresAtUtc <= now)
        {
            throw new InvalidOperationException("Refresh token is expired or revoked.");
        }

        existing.RevokedAtUtc = now;
        existing.RevokeReason = "rotated";

        var tokenPair = await IssueTokenPairAsync(existing.User, cancellationToken);
        return new RefreshTokenResponse(
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAtUtc);
    }

    public async Task RevokeAsync(RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken)
            ?? throw new KeyNotFoundException("Refresh token was not found.");

        if (!token.RevokedAtUtc.HasValue)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokeReason = "manual-revoke";
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc)> IssueTokenPairAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_jwtOptions.ExpiresInMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenValue = GenerateRefreshTokenValue();
        var refreshTokenExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenExpiresInDays);
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (accessToken, expiresAtUtc, refreshTokenValue, refreshTokenExpiresAtUtc);
    }

    private static string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
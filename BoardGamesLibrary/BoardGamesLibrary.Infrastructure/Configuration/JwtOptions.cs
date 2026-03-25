namespace BoardGamesLibrary.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
    public int RefreshTokenExpiresInDays { get; set; } = 7;
}
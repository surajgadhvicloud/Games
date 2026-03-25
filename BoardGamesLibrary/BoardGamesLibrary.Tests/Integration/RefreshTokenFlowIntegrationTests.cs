using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Tests.Integration;

public class RefreshTokenFlowIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private const string Password = "P@ssw0rd123";
    private readonly TestWebApplicationFactory _factory;

    public RefreshTokenFlowIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ThenRefresh_ReturnsNewTokenPair()
    {
        using var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", Password));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login!.RefreshToken));
        refreshResponse.EnsureSuccessStatusCode();
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.False(string.IsNullOrWhiteSpace(refreshed!.AccessToken));
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task Revoke_PreventsFurtherRefresh()
    {
        using var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("manager", Password));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        var revokeResponse = await client.PostAsJsonAsync("/api/auth/revoke", new RevokeTokenRequest(login!.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login.RefreshToken));
        Assert.Equal(HttpStatusCode.Conflict, refreshResponse.StatusCode);
    }
}
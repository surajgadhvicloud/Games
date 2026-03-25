using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGamesLibrary.Tests.Integration;

public class RoleAccessMatrixIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private const string Password = "P@ssw0rd123";
    private readonly TestWebApplicationFactory _factory;

    public RoleAccessMatrixIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_CanLogin_ButCannotAccessProtectedEndpoints()
    {
        using var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", Password));
        var membersResponse = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, membersResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateUser()
    {
        using var client = await CreateAuthorizedClientAsync("admin");

        var request = new CreateUserRequest(
            "Test",
            "AdminCreated",
            $"admin.created.{Guid.NewGuid():N}@example.com",
            $"admin_created_{Guid.NewGuid():N}"[..24],
            "P@ssw0rd123",
            UserRole.DataEntry);

        var response = await client.PostAsJsonAsync("/api/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CanCreateMember_AndCanCreateUser()
    {
        using var client = await CreateAuthorizedClientAsync("manager");

        var memberResponse = await client.PostAsJsonAsync("/api/members", new CreateMemberRequest(
            "Manager",
            null,
            "Created",
            "Address",
            "9111111111",
            $"manager.member.{Guid.NewGuid():N}@example.com",
            UserType.Regular));

        var userResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
            "Manager",
            "Created",
            $"manager.created.{Guid.NewGuid():N}@example.com",
            $"manager_created_{Guid.NewGuid():N}"[..24],
            "P@ssw0rd123",
            UserRole.DataEntry));

        Assert.Equal(HttpStatusCode.Created, memberResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
    }

    [Fact]
    public async Task DataEntry_HasReadOnlyOnCatalog_AndWriteOnGameIssuesOnly()
    {
        using var client = await CreateAuthorizedClientAsync("dataentry");

        var listBoardGames = await client.GetAsync("/api/boardgames");
        var listMembers = await client.GetAsync("/api/members");

        var createBoardGame = await client.PostAsJsonAsync("/api/boardgames", new CreateBoardGameRequest(
            "Forbidden Game",
            "v1",
            2,
            4,
            1000));

        var createInventory = await client.PostAsJsonAsync("/api/inventories", new CreateInventoryRequest(
            1,
            false,
            2,
            2));

        var listUsers = await client.GetAsync("/api/users");

        var gameIssueRequest = await BuildGameIssueRequestAsync();
        var createGameIssue = await client.PostAsJsonAsync("/api/gameissues", gameIssueRequest);

        Assert.Equal(HttpStatusCode.OK, listBoardGames.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listMembers.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createBoardGame.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createInventory.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, listUsers.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createGameIssue.StatusCode);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(string username)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, Password));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        return client;
    }

    private async Task<CreateGameIssueRequest> BuildGameIssueRequestAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BoardGamesDbContext>();

        var memberId = dbContext.Members.Select(x => x.Id).First();
        var boardGameId = dbContext.BoardGames.Select(x => x.Id).First();

        return new CreateGameIssueRequest(
            boardGameId,
            memberId,
            DateTime.UtcNow,
            null,
            GameCondition.Mint);
    }
}
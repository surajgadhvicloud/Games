using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresNormalizedUsername_AndRejectsCaseInsensitiveDuplicates()
    {
        await using var harness = await UserServiceHarness.CreateAsync();

        var created = await harness.Service.CreateAsync(
            new CreateUserRequest(
                FirstName: "Test",
                LastName: "User",
                Email: "test.user@example.com",
                Username: "  NewUser  ",
                Password: "P@ssw0rd123",
                Role: UserRole.DataEntry),
            CancellationToken.None);

        Assert.Equal("newuser", created.Username);

        var action = () => harness.Service.CreateAsync(
            new CreateUserRequest(
                FirstName: "Another",
                LastName: "User",
                Email: "another.user@example.com",
                Username: "NEWUSER",
                Password: "P@ssw0rd123",
                Role: UserRole.DataEntry),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task UpdateAsync_RejectsCaseInsensitiveUsernameConflict()
    {
        await using var harness = await UserServiceHarness.CreateAsync();

        var first = await harness.Service.CreateAsync(
            new CreateUserRequest("First", "User", "first.user@example.com", "firstuser", "P@ssw0rd123", UserRole.DataEntry),
            CancellationToken.None);

        var second = await harness.Service.CreateAsync(
            new CreateUserRequest("Second", "User", "second.user@example.com", "seconduser", "P@ssw0rd123", UserRole.DataEntry),
            CancellationToken.None);

        var action = () => harness.Service.UpdateAsync(
            second.Id,
            new UpdateUserRequest(
                FirstName: "Second",
                LastName: "User",
                Email: "second.user@example.com",
                Username: " FIRSTUSER ",
                Password: null,
                Role: UserRole.DataEntry),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);

        var unchanged = await harness.Service.GetAsync(second.Id, CancellationToken.None);
        Assert.Equal("seconduser", unchanged.Username);
    }

    private sealed class UserServiceHarness : IAsyncDisposable
    {
        private UserServiceHarness(BoardGamesDbContext dbContext)
        {
            DbContext = dbContext;
            Service = new UserService(
                dbContext,
                new TestCurrentUserService(),
                new PasswordHasher<User>(),
                new UnitOfWork(dbContext));
        }

        public BoardGamesDbContext DbContext { get; }
        public UserService Service { get; }

        public static async Task<UserServiceHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"UserServiceTests-{Guid.NewGuid()}")
                .Options;

            var dbContext = new BoardGamesDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return new UserServiceHarness(dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string GetUsername() => "test-user";
    }
}

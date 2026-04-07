using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

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
        Assert.Equal("hashed:P@ssw0rd123", await harness.GetPasswordHashAsync(created.Id));
        harness.PasswordHasherMock.Verify(x => x.HashPassword(It.IsAny<User>(), "P@ssw0rd123"), Times.Once);
        harness.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

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
    public async Task CreateAsync_RejectsCaseInsensitiveEmailDuplicates()
    {
        await using var harness = await UserServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateUserRequest("First", "User", "duplicate@example.com", "firstuser", "P@ssw0rd123", UserRole.DataEntry),
            CancellationToken.None);

        var action = () => harness.Service.CreateAsync(
            new CreateUserRequest("Second", "User", " DUPLICATE@EXAMPLE.COM ", "seconduser", "P@ssw0rd123", UserRole.DataEntry),
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

    [Fact]
    public async Task UpdateAsync_UpdatesPassword_WhenProvided()
    {
        await using var harness = await UserServiceHarness.CreateAsync();
        var created = await harness.Service.CreateAsync(
            new CreateUserRequest("First", "User", "first.user@example.com", "firstuser", "old-pass", UserRole.DataEntry),
            CancellationToken.None);

        await harness.Service.UpdateAsync(
            created.Id,
            new UpdateUserRequest("First", "User", "first.user@example.com", "firstuser", "new-pass", UserRole.Manager),
            CancellationToken.None);

        Assert.Equal("hashed:new-pass", await harness.GetPasswordHashAsync(created.Id));
        harness.PasswordHasherMock.Verify(x => x.HashPassword(It.IsAny<User>(), "new-pass"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotHash_WhenPasswordMissing()
    {
        await using var harness = await UserServiceHarness.CreateAsync();
        var created = await harness.Service.CreateAsync(
            new CreateUserRequest("First", "User", "first.user@example.com", "firstuser", "old-pass", UserRole.DataEntry),
            CancellationToken.None);

        harness.PasswordHasherMock.Invocations.Clear();

        await harness.Service.UpdateAsync(
            created.Id,
            new UpdateUserRequest("First", "User", "first.user@example.com", "firstuser", null, UserRole.Manager),
            CancellationToken.None);

        harness.PasswordHasherMock.Verify(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUserMissing()
    {
        await using var harness = await UserServiceHarness.CreateAsync();

        var action = () => harness.Service.UpdateAsync(
            111,
            new UpdateUserRequest("First", "User", "missing@example.com", "missing", null, UserRole.DataEntry),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ListAsync_ReturnsUsersOrderedByUsername()
    {
        await using var harness = await UserServiceHarness.CreateAsync();
        await harness.Service.CreateAsync(new CreateUserRequest("B", "User", "b@example.com", "beta", "pass", UserRole.DataEntry), CancellationToken.None);
        await harness.Service.CreateAsync(new CreateUserRequest("A", "User", "a@example.com", "alpha", "pass", UserRole.DataEntry), CancellationToken.None);

        var result = await harness.Service.ListAsync(page: 1, pageSize: 10, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("alpha", result.Items[0].Username);
        Assert.Equal("beta", result.Items[1].Username);
    }

    [Fact]
    public async Task GetAsync_Throws_WhenMissing()
    {
        await using var harness = await UserServiceHarness.CreateAsync();

        var action = () => harness.Service.GetAsync(404, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class UserServiceHarness : IAsyncDisposable
    {
        private UserServiceHarness(
            BoardGamesDbContext dbContext,
            Mock<ICurrentUserService> currentUserServiceMock,
            Mock<IPasswordHasher<User>> passwordHasherMock,
            Mock<IUnitOfWork> unitOfWorkMock)
        {
            DbContext = dbContext;
            CurrentUserServiceMock = currentUserServiceMock;
            PasswordHasherMock = passwordHasherMock;
            UnitOfWorkMock = unitOfWorkMock;
            Service = new UserService(
                dbContext,
                currentUserServiceMock.Object,
                passwordHasherMock.Object,
                unitOfWorkMock.Object);
        }

        public BoardGamesDbContext DbContext { get; }
        public UserService Service { get; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; }
        public Mock<IPasswordHasher<User>> PasswordHasherMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public static async Task<UserServiceHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"UserServiceTests-{Guid.NewGuid()}")
                .Options;

            var dbContext = new BoardGamesDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetUsername()).Returns("test-user");

            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            passwordHasherMock
                .Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                .Returns((User _, string password) => $"hashed:{password}");

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => dbContext.SaveChangesAsync(ct));

            return new UserServiceHarness(dbContext, currentUserServiceMock, passwordHasherMock, unitOfWorkMock);
        }

        public async Task<string> GetPasswordHashAsync(int userId)
        {
            var entity = await DbContext.Users.FirstAsync(x => x.Id == userId);
            return entity.PasswordHash;
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}

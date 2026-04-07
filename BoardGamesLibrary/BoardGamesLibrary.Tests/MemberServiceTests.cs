using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BoardGamesLibrary.Tests;

public class MemberServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesEmailAndTrimsNames()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        var result = await harness.Service.CreateAsync(
            new CreateMemberRequest(
                FirstName: "  Priya ",
                MiddleName: " R ",
                LastName: " Menon ",
                Address: " 10 Main Road ",
                PhoneNumber: " 9000000001 ",
                Email: " PRIYA.MENON@Example.COM ",
                TypeOfUser: UserType.Premium),
            CancellationToken.None);

        Assert.Equal("priya.menon@example.com", result.Email);
        Assert.Equal("Priya", result.FirstName);
        Assert.Equal("R", result.MiddleName);
        harness.CurrentUserServiceMock.Verify(x => x.GetUsername(), Times.Once);
        harness.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailAlreadyExists()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateMemberRequest("A", null, "B", "Addr", "9000000000", "duplicate@example.com", UserType.Regular),
            CancellationToken.None);

        var action = () => harness.Service.CreateAsync(
            new CreateMemberRequest("C", null, "D", "Addr", "9000000001", "duplicate@example.com", UserType.Premium),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenMemberNotFound()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        var action = () => harness.Service.UpdateAsync(
            id: 9999,
            new UpdateMemberRequest("F", null, "L", "Addr", "9111111111", "missing@example.com", UserType.Regular),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEmailAlreadyUsedByAnotherMember()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        var first = await harness.Service.CreateAsync(
            new CreateMemberRequest("A", null, "B", "Addr", "9000000000", "first@example.com", UserType.Regular),
            CancellationToken.None);

        var second = await harness.Service.CreateAsync(
            new CreateMemberRequest("C", null, "D", "Addr", "9000000001", "second@example.com", UserType.Premium),
            CancellationToken.None);

        var action = () => harness.Service.UpdateAsync(
            second.Id,
            new UpdateMemberRequest("C", null, "D", "Addr", "9000000001", first.Email, UserType.Premium),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMember_WhenValid()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        var created = await harness.Service.CreateAsync(
            new CreateMemberRequest("A", null, "B", "Addr", "9000000000", "member@example.com", UserType.Regular),
            CancellationToken.None);

        var updated = await harness.Service.UpdateAsync(
            created.Id,
            new UpdateMemberRequest("Updated", "M", "Member", "New Addr", "9999999999", "updated@example.com", UserType.Premium),
            CancellationToken.None);

        Assert.Equal("updated@example.com", updated.Email);
        Assert.Equal(UserType.Premium, updated.TypeOfUser);
    }

    [Fact]
    public async Task ListAsync_ReturnsSortedMembers()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateMemberRequest("Zed", null, "Zulu", "Addr", "9000000000", "z@example.com", UserType.Regular),
            CancellationToken.None);

        await harness.Service.CreateAsync(
            new CreateMemberRequest("Amy", null, "Alpha", "Addr", "9000000001", "a@example.com", UserType.Regular),
            CancellationToken.None);

        var result = await harness.Service.ListAsync(page: 1, pageSize: 10, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Amy", result.Items[0].FirstName);
        Assert.Equal("Zed", result.Items[1].FirstName);
    }

    [Fact]
    public async Task GetAsync_Throws_WhenMemberMissing()
    {
        await using var harness = await MemberServiceHarness.CreateAsync();

        var action = () => harness.Service.GetAsync(404, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class MemberServiceHarness : IAsyncDisposable
    {
        private MemberServiceHarness(
            BoardGamesDbContext dbContext,
            Mock<ICurrentUserService> currentUserServiceMock,
            Mock<IUnitOfWork> unitOfWorkMock)
        {
            DbContext = dbContext;
            CurrentUserServiceMock = currentUserServiceMock;
            UnitOfWorkMock = unitOfWorkMock;
            Service = new MemberService(dbContext, currentUserServiceMock.Object, unitOfWorkMock.Object);
        }

        public BoardGamesDbContext DbContext { get; }
        public MemberService Service { get; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }

        public static async Task<MemberServiceHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"MemberServiceTests-{Guid.NewGuid()}")
                .Options;

            var dbContext = new BoardGamesDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetUsername()).Returns("test-user");

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => dbContext.SaveChangesAsync(ct));

            return new MemberServiceHarness(dbContext, currentUserServiceMock, unitOfWorkMock);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}

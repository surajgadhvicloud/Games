using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BoardGamesLibrary.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task CreateAsync_Succeeds_ForExistingBoardGame()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        var result = await harness.Service.CreateAsync(
            new CreateInventoryRequest(
                BoardGameId: harness.BoardGameId,
                IsMissingOrBroken: false,
                TotalInventory: 4,
                AvailableInventory: 4),
            CancellationToken.None);

        Assert.Equal(harness.BoardGameId, result.BoardGameId);
        Assert.Equal(4, result.TotalInventory);
        harness.CurrentUserServiceMock.Verify(x => x.GetUsername(), Times.Once);
        harness.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenInventoryAlreadyExists()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 2, 2),
            CancellationToken.None);

        var action = () => harness.Service.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 3, 3),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenBoardGameDoesNotExist()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        var action = () => harness.Service.CreateAsync(
            new CreateInventoryRequest(99999, false, 2, 2),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenDecreasingTotalWithActiveIssues()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 5, 4),
            CancellationToken.None);

        harness.DbContext.GameIssues.Add(new GameIssue
        {
            BoardGameId = harness.BoardGameId,
            MemberId = harness.MemberId,
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = DateTime.UtcNow.AddDays(14),
            ConditionGivenOut = GameCondition.Mint,
            Status = GameIssueStatus.Active,
            OverdueCharges = 0
        });
        await harness.DbContext.SaveChangesAsync();

        var action = () => harness.Service.UpdateAsync(
            harness.BoardGameId,
            new UpdateInventoryRequest(false, 3, 3),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesInventory_WhenNoActiveIssues()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 5, 5),
            CancellationToken.None);

        var updated = await harness.Service.UpdateAsync(
            harness.BoardGameId,
            new UpdateInventoryRequest(true, 7, 6),
            CancellationToken.None);

        Assert.True(updated.IsMissingOrBroken);
        Assert.Equal(7, updated.TotalInventory);
        Assert.Equal(6, updated.AvailableInventory);
    }

    [Fact]
    public async Task ListAsync_ReturnsPagedInventories()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();
        await harness.Service.CreateAsync(new CreateInventoryRequest(harness.BoardGameId, false, 4, 4), CancellationToken.None);

        var secondGame = new BoardGame
        {
            GameName = "Second",
            Version = "Base",
            MinPlayers = 2,
            MaxPlayers = 4,
            Price = 800
        };
        harness.DbContext.BoardGames.Add(secondGame);
        await harness.DbContext.SaveChangesAsync();

        await harness.Service.CreateAsync(new CreateInventoryRequest(secondGame.Id, false, 2, 2), CancellationToken.None);

        var result = await harness.Service.ListAsync(page: 1, pageSize: 1, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByBoardGameIdAsync_Throws_WhenMissing()
    {
        await using var harness = await InventoryServiceHarness.CreateAsync();

        var action = () => harness.Service.GetByBoardGameIdAsync(8080, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class InventoryServiceHarness : IAsyncDisposable
    {
        private InventoryServiceHarness(
            BoardGamesDbContext dbContext,
            Mock<ICurrentUserService> currentUserServiceMock,
            Mock<IUnitOfWork> unitOfWorkMock)
        {
            DbContext = dbContext;
            CurrentUserServiceMock = currentUserServiceMock;
            UnitOfWorkMock = unitOfWorkMock;
            Service = new InventoryService(dbContext, currentUserServiceMock.Object, unitOfWorkMock.Object);
        }

        public BoardGamesDbContext DbContext { get; }
        public InventoryService Service { get; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public int MemberId { get; private set; }
        public int BoardGameId { get; private set; }

        public static async Task<InventoryServiceHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"InventoryServiceTests-{Guid.NewGuid()}")
                .Options;

            var dbContext = new BoardGamesDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var member = new Member
            {
                FirstName = "Seed",
                LastName = "Member",
                Address = "Seed Address",
                PhoneNumber = "9555555555",
                Email = "seed.member@example.com",
                TypeOfUser = UserType.Regular
            };

            var boardGame = new BoardGame
            {
                GameName = "Pandemic",
                Version = "Base",
                MinPlayers = 2,
                MaxPlayers = 4,
                Price = 2500
            };

            dbContext.Members.Add(member);
            dbContext.BoardGames.Add(boardGame);
            await dbContext.SaveChangesAsync();

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetUsername()).Returns("test-user");

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => dbContext.SaveChangesAsync(ct));

            return new InventoryServiceHarness(dbContext, currentUserServiceMock, unitOfWorkMock)
            {
                MemberId = member.Id,
                BoardGameId = boardGame.Id
            };
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}

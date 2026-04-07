using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Configuration;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Moq;

namespace BoardGamesLibrary.Tests;

public class GameIssueServiceTests
{
    [Fact]
    public async Task CreateAsync_DecrementsInventoryAndCreatesActiveIssue_AndUsesCurrentUser()
    {
        await using var harness = await TestHarness.CreateAsync();
        const string photoBeforeIssue = " https://s3.amazonaws.com/library-bucket/games/monopoly-before.jpg ";

        var result = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.RegularMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: null,
                ConditionGivenOut: GameCondition.Mint,
                PhotoUrlBeforeIssue: photoBeforeIssue),
            CancellationToken.None);

        var inventory = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);
        var issue = await harness.DbContext.GameIssues.FirstAsync(x => x.Id == result.Id);

        Assert.Equal(GameIssueStatus.Active, result.Status);
        Assert.Equal(1, inventory.AvailableInventory);
        Assert.Equal(photoBeforeIssue.Trim(), result.PhotoUrlBeforeIssue);
        Assert.Equal("test-user", issue.ModifiedByUser);
        Assert.Null(result.PhotoUrlAfterReturn);

        harness.CurrentUserServiceMock.Verify(x => x.GetUsername(), Times.AtLeastOnce);
        harness.UnitOfWorkMock.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<GameIssueResponse>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        harness.UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenMemberMissing()
    {
        await using var harness = await TestHarness.CreateAsync();

        var action = () => harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                999_999,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: null,
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenInventoryUnavailable()
    {
        await using var harness = await TestHarness.CreateAsync();
        var inventory = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);
        inventory.AvailableInventory = 0;
        await harness.DbContext.SaveChangesAsync();

        var action = () => harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.RegularMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: null,
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenInventoryFlaggedMissingOrBroken()
    {
        await using var harness = await TestHarness.CreateAsync();
        var inventory = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);
        inventory.IsMissingOrBroken = true;
        await harness.DbContext.SaveChangesAsync();

        var action = () => harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.RegularMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: null,
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenRegularMemberExceedsActiveLimit()
    {
        await using var harness = await TestHarness.CreateAsync();

        await harness.Service.CreateAsync(
            new CreateGameIssueRequest(harness.BoardGameId, harness.RegularMemberId, DateTime.UtcNow, null, GameCondition.Mint),
            CancellationToken.None);

        var secondGame = await harness.CreateExtraGameWithInventoryAsync("Azul", "Base", 2);
        await harness.Service.CreateAsync(
            new CreateGameIssueRequest(secondGame.boardGameId, harness.RegularMemberId, DateTime.UtcNow, null, GameCondition.Mint),
            CancellationToken.None);

        var thirdGame = await harness.CreateExtraGameWithInventoryAsync("Carcassonne", "Base", 2);

        var action = () => harness.Service.CreateAsync(
            new CreateGameIssueRequest(thirdGame.boardGameId, harness.RegularMemberId, DateTime.UtcNow, null, GameCondition.Mint),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("max active issue limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_UsesPremiumLoanDays_WhenEndDateMissing()
    {
        await using var harness = await TestHarness.CreateAsync();
        var startDate = DateTime.UtcNow.Date;

        var result = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.PremiumMemberId,
                StartDateUtc: startDate,
                EndDateUtc: null,
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        Assert.Equal(startDate.AddDays(harness.Options.PremiumLoanDays), result.EndDateUtc);
    }

    [Fact]
    public async Task UpdateAsync_AppliesOverdueCharges_WhenReturnedLate()
    {
        await using var harness = await TestHarness.CreateAsync();
        const string photoAfterReturn = "https://storageaccount.blob.core.windows.net/boardgames/monopoly-after.jpg";
        var issue = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.PremiumMemberId,
                StartDateUtc: DateTime.UtcNow.AddDays(-35),
                EndDateUtc: DateTime.UtcNow.AddDays(-5),
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        var returned = await harness.Service.UpdateAsync(
            issue.Id,
            new UpdateGameIssueRequest(
                ReturnDateUtc: DateTime.UtcNow,
                ConditionGivenIn: GameCondition.CompleteNotMint,
                PhotoUrlAfterReturn: photoAfterReturn),
            CancellationToken.None);

        Assert.Equal(GameIssueStatus.Overdue, returned.Status);
        Assert.Equal(5 * 250m, returned.OverdueCharges);
        Assert.Equal(photoAfterReturn, returned.PhotoUrlAfterReturn);
    }

    [Fact]
    public async Task UpdateAsync_SetsReturnedAndIncrementsInventory_WhenReturnedOnTime()
    {
        await using var harness = await TestHarness.CreateAsync();
        var inventoryBefore = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);

        var issue = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.PremiumMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: DateTime.UtcNow.AddDays(3),
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        var updated = await harness.Service.UpdateAsync(
            issue.Id,
            new UpdateGameIssueRequest(
                ReturnDateUtc: DateTime.UtcNow,
                ConditionGivenIn: GameCondition.CompleteNotMint),
            CancellationToken.None);

        var inventory = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);
        Assert.Equal(GameIssueStatus.Returned, updated.Status);
        Assert.Equal(0m, updated.OverdueCharges);
        Assert.Equal(inventoryBefore.AvailableInventory, inventory.AvailableInventory);
    }

    [Fact]
    public async Task UpdateAsync_FlagsInventory_WhenReturnedBroken()
    {
        await using var harness = await TestHarness.CreateAsync();
        var issue = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.PremiumMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: DateTime.UtcNow.AddDays(1),
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        await harness.Service.UpdateAsync(
            issue.Id,
            new UpdateGameIssueRequest(
                ReturnDateUtc: DateTime.UtcNow,
                ConditionGivenIn: GameCondition.Broken),
            CancellationToken.None);

        var inventory = await harness.DbContext.Inventories.FirstAsync(x => x.BoardGameId == harness.BoardGameId);
        Assert.True(inventory.IsMissingOrBroken);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenIssueAlreadyReturned()
    {
        await using var harness = await TestHarness.CreateAsync();
        var issue = await harness.Service.CreateAsync(
            new CreateGameIssueRequest(
                harness.BoardGameId,
                harness.PremiumMemberId,
                StartDateUtc: DateTime.UtcNow,
                EndDateUtc: DateTime.UtcNow.AddDays(2),
                ConditionGivenOut: GameCondition.Mint),
            CancellationToken.None);

        await harness.Service.UpdateAsync(
            issue.Id,
            new UpdateGameIssueRequest(DateTime.UtcNow, GameCondition.CompleteNotMint),
            CancellationToken.None);

        var action = () => harness.Service.UpdateAsync(
            issue.Id,
            new UpdateGameIssueRequest(DateTime.UtcNow, GameCondition.CompleteNotMint),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task ListAsync_ReturnsPagedResult_OrderedByStartDateDesc()
    {
        await using var harness = await TestHarness.CreateAsync();
        var now = DateTime.UtcNow;

        await harness.Service.CreateAsync(
            new CreateGameIssueRequest(harness.BoardGameId, harness.PremiumMemberId, now.AddDays(-5), now.AddDays(2), GameCondition.Mint),
            CancellationToken.None);

        var game2 = await harness.CreateExtraGameWithInventoryAsync("Gloomhaven", "JOTL", 2);
        await harness.Service.CreateAsync(
            new CreateGameIssueRequest(game2.boardGameId, harness.PremiumMemberId, now.AddDays(-2), now.AddDays(2), GameCondition.Mint),
            CancellationToken.None);

        var paged = await harness.Service.ListAsync(page: 1, pageSize: 1, CancellationToken.None);

        Assert.Equal(2, paged.TotalCount);
        Assert.Single(paged.Items);
        Assert.Equal(now.AddDays(-2).Date, paged.Items[0].StartDateUtc.Date);
    }

    [Fact]
    public async Task GetAsync_Throws_WhenIssueMissing()
    {
        await using var harness = await TestHarness.CreateAsync();

        var action = () => harness.Service.GetAsync(404, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class TestHarness : IAsyncDisposable
    {
        private TestHarness(
            BoardGamesDbContext dbContext,
            GameIssueService service,
            Mock<ICurrentUserService> currentUserServiceMock,
            Mock<IUnitOfWork> unitOfWorkMock,
            BusinessRulesOptions options)
        {
            DbContext = dbContext;
            Service = service;
            CurrentUserServiceMock = currentUserServiceMock;
            UnitOfWorkMock = unitOfWorkMock;
            Options = options;
        }

        public BoardGamesDbContext DbContext { get; }
        public GameIssueService Service { get; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public BusinessRulesOptions Options { get; }
        public int RegularMemberId { get; private set; }
        public int PremiumMemberId { get; private set; }
        public int BoardGameId { get; private set; }

        public static async Task<TestHarness> CreateAsync()
        {
            var dbOptions = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"BoardGamesTests-{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new BoardGamesDbContext(dbOptions);
            await dbContext.Database.EnsureCreatedAsync();

            var regularMember = new Member
            {
                FirstName = "Regular",
                LastName = "Member",
                Address = "Address",
                PhoneNumber = "9999999999",
                Email = "regular.member@example.com",
                TypeOfUser = UserType.Regular
            };

            var premiumMember = new Member
            {
                FirstName = "Premium",
                LastName = "Member",
                Address = "Address",
                PhoneNumber = "8888888888",
                Email = "premium.member@example.com",
                TypeOfUser = UserType.Premium
            };

            var game = new BoardGame
            {
                GameName = "Monopoly",
                Version = "Classic",
                MinPlayers = 2,
                MaxPlayers = 6,
                Price = 1499
            };

            dbContext.Members.AddRange(regularMember, premiumMember);
            dbContext.BoardGames.Add(game);
            await dbContext.SaveChangesAsync();

            dbContext.Inventories.Add(new Inventory
            {
                BoardGameId = game.Id,
                IsMissingOrBroken = false,
                TotalInventory = 2,
                AvailableInventory = 2
            });

            await dbContext.SaveChangesAsync();

            var options = new BusinessRulesOptions
            {
                RegularMaxActiveIssues = 2,
                PremiumMaxActiveIssues = 5,
                RegularLoanDays = 14,
                PremiumLoanDays = 30,
                OverdueDailyFeeInr = 250m
            };

            var optionsWrapper = new Mock<IOptions<BusinessRulesOptions>>();
            optionsWrapper.Setup(x => x.Value).Returns(options);

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.GetUsername()).Returns("test-user");

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => dbContext.SaveChangesAsync(ct));
            unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<CancellationToken, Task<GameIssueResponse>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns((Func<CancellationToken, Task<GameIssueResponse>> operation, CancellationToken ct) => operation(ct));

            var service = new GameIssueService(dbContext, currentUserServiceMock.Object, optionsWrapper.Object, unitOfWorkMock.Object);

            return new TestHarness(dbContext, service, currentUserServiceMock, unitOfWorkMock, options)
            {
                RegularMemberId = regularMember.Id,
                PremiumMemberId = premiumMember.Id,
                BoardGameId = game.Id
            };
        }

        public async Task<(int boardGameId, int inventoryId)> CreateExtraGameWithInventoryAsync(string gameName, string version, int total)
        {
            var game = new BoardGame
            {
                GameName = gameName,
                Version = version,
                MinPlayers = 2,
                MaxPlayers = 4,
                Price = 1000
            };

            DbContext.BoardGames.Add(game);
            await DbContext.SaveChangesAsync();

            var inventory = new Inventory
            {
                BoardGameId = game.Id,
                IsMissingOrBroken = false,
                TotalInventory = total,
                AvailableInventory = total
            };

            DbContext.Inventories.Add(inventory);
            await DbContext.SaveChangesAsync();

            return (game.Id, inventory.Id);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}
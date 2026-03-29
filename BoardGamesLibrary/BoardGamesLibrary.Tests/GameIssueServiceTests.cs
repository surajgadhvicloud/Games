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

namespace BoardGamesLibrary.Tests;

public class GameIssueServiceTests
{
    [Fact]
    public async Task CreateAsync_DecrementsInventoryAndCreatesActiveIssue()
    {
        await using var harness = await TestHarness.CreateAsync();
        const string photoBeforeIssue = "https://s3.amazonaws.com/library-bucket/games/monopoly-before.jpg";

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

        Assert.Equal(GameIssueStatus.Active, result.Status);
        Assert.Equal(1, inventory.AvailableInventory);
        Assert.Equal(photoBeforeIssue, result.PhotoUrlBeforeIssue);
        Assert.Null(result.PhotoUrlAfterReturn);
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

    private sealed class TestHarness : IAsyncDisposable
    {
        private TestHarness(BoardGamesDbContext dbContext, GameIssueService service)
        {
            DbContext = dbContext;
            Service = service;
        }

        public BoardGamesDbContext DbContext { get; }
        public GameIssueService Service { get; }
        public int RegularMemberId { get; private set; }
        public int PremiumMemberId { get; private set; }
        public int BoardGameId { get; private set; }

        public static async Task<TestHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"BoardGamesTests-{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new BoardGamesDbContext(options);
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

            var optionsWrapper = Options.Create(new BusinessRulesOptions
            {
                RegularMaxActiveIssues = 2,
                PremiumMaxActiveIssues = 5,
                RegularLoanDays = 14,
                PremiumLoanDays = 30,
                OverdueDailyFeeInr = 250m
            });

            var service = new GameIssueService(dbContext, new TestCurrentUserService(), optionsWrapper);

            return new TestHarness(dbContext, service)
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

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string GetUsername() => "test-user";
    }
}
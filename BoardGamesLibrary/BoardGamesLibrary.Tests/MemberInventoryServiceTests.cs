using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using BoardGamesLibrary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Tests;

public class MemberInventoryServiceTests
{
    [Fact]
    public async Task Member_CreateAsync_NormalizesEmailAndPersists()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        var result = await harness.MemberService.CreateAsync(
            new CreateMemberRequest(
                FirstName: "  Priya ",
                MiddleName: " R ",
                LastName: "  Menon ",
                Address: "  10 Main Road ",
                PhoneNumber: " 9000000001 ",
                Email: " PRIYA.MENON@Example.COM ",
                TypeOfUser: UserType.Premium),
            CancellationToken.None);

        Assert.Equal("priya.menon@example.com", result.Email);
        Assert.Equal("Priya", result.FirstName);
    }

    [Fact]
    public async Task Member_CreateAsync_Throws_WhenEmailAlreadyExists()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        await harness.MemberService.CreateAsync(
            new CreateMemberRequest("A", null, "B", "Addr", "9000000000", "duplicate@example.com", UserType.Regular),
            CancellationToken.None);

        var action = () => harness.MemberService.CreateAsync(
            new CreateMemberRequest("C", null, "D", "Addr", "9000000001", "duplicate@example.com", UserType.Premium),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task Member_UpdateAsync_Throws_WhenMemberNotFound()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        var action = () => harness.MemberService.UpdateAsync(
            id: 9999,
            new UpdateMemberRequest("F", null, "L", "Addr", "9111111111", "missing@example.com", UserType.Regular),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task Inventory_CreateAsync_Succeeds_ForExistingBoardGame()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        var result = await harness.InventoryService.CreateAsync(
            new CreateInventoryRequest(
                BoardGameId: harness.BoardGameId,
                IsMissingOrBroken: false,
                TotalInventory: 4,
                AvailableInventory: 4),
            CancellationToken.None);

        Assert.Equal(harness.BoardGameId, result.BoardGameId);
        Assert.Equal(4, result.TotalInventory);
    }

    [Fact]
    public async Task Inventory_CreateAsync_Throws_WhenInventoryAlreadyExists()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        await harness.InventoryService.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 2, 2),
            CancellationToken.None);

        var action = () => harness.InventoryService.CreateAsync(
            new CreateInventoryRequest(harness.BoardGameId, false, 3, 3),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task Inventory_UpdateAsync_Throws_WhenDecreasingTotalWithActiveIssues()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        await harness.InventoryService.CreateAsync(
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

        var action = () => harness.InventoryService.UpdateAsync(
            harness.BoardGameId,
            new UpdateInventoryRequest(false, 3, 3),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task Inventory_CreateAsync_Throws_WhenBoardGameDoesNotExist()
    {
        await using var harness = await MemberInventoryHarness.CreateAsync();

        var action = () => harness.InventoryService.CreateAsync(
            new CreateInventoryRequest(99999, false, 2, 2),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class MemberInventoryHarness : IAsyncDisposable
    {
        private MemberInventoryHarness(BoardGamesDbContext dbContext)
        {
            DbContext = dbContext;
            var currentUserService = new TestCurrentUserService();
            var unitOfWork = new UnitOfWork(dbContext);
            MemberService = new MemberService(dbContext, currentUserService, unitOfWork);
            InventoryService = new InventoryService(dbContext, currentUserService, unitOfWork);
        }

        public BoardGamesDbContext DbContext { get; }
        public MemberService MemberService { get; }
        public InventoryService InventoryService { get; }
        public int MemberId { get; private set; }
        public int BoardGameId { get; private set; }

        public static async Task<MemberInventoryHarness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<BoardGamesDbContext>()
                .UseInMemoryDatabase($"MemberInventoryTests-{Guid.NewGuid()}")
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

            return new MemberInventoryHarness(dbContext)
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

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string GetUsername() => "test-user";
    }
}
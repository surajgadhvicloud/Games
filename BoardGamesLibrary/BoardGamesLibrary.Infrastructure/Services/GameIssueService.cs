using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Configuration;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BoardGamesLibrary.Infrastructure.Services;

public class GameIssueService(
    BoardGamesDbContext dbContext,
    ICurrentUserService currentUserService,
    IOptions<BusinessRulesOptions> options) : IGameIssueService
{
    private readonly BusinessRulesOptions _businessRules = options.Value;

    public async Task<GameIssueResponse> CreateAsync(CreateGameIssueRequest request, CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var member = await dbContext.Members.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Member {request.UserId} was not found.");
        var inventory = await dbContext.Inventories.FirstOrDefaultAsync(x => x.BoardGameId == request.BoardGameId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for board game {request.BoardGameId} was not found.");

        if (inventory.IsMissingOrBroken)
        {
            throw new InvalidOperationException("Game is flagged as missing or broken and cannot be issued.");
        }

        if (inventory.AvailableInventory <= 0)
        {
            throw new InvalidOperationException("No available inventory to issue.");
        }

        var maxLimit = member.TypeOfUser == UserType.Premium
            ? _businessRules.PremiumMaxActiveIssues
            : _businessRules.RegularMaxActiveIssues;

        var activeIssueCount = await dbContext.GameIssues
            .CountAsync(x => x.MemberId == member.Id && x.ReturnDateUtc == null, cancellationToken);
        if (activeIssueCount >= maxLimit)
        {
            throw new InvalidOperationException($"Member reached max active issue limit ({maxLimit}).");
        }

        var startDate = request.StartDateUtc ?? DateTime.UtcNow;
        var endDate = request.EndDateUtc ?? startDate.AddDays(
            member.TypeOfUser == UserType.Premium
                ? _businessRules.PremiumLoanDays
                : _businessRules.RegularLoanDays);

        var issue = new GameIssue
        {
            BoardGameId = request.BoardGameId,
            MemberId = request.UserId,
            StartDateUtc = startDate,
            EndDateUtc = endDate,
            ConditionGivenOut = request.ConditionGivenOut,
            Status = GameIssueStatus.Active,
            OverdueCharges = 0,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedByUser = currentUserService.GetUsername()
        };

        inventory.AvailableInventory -= 1;
        inventory.UpdatedAtUtc = DateTime.UtcNow;
        inventory.ModifiedByUser = currentUserService.GetUsername();

        dbContext.GameIssues.Add(issue);
        await dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return ToResponse(issue);
    }

    public async Task<GameIssueResponse> UpdateAsync(int id, UpdateGameIssueRequest request, CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var issue = await dbContext.GameIssues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Game issue {id} was not found.");

        if (issue.ReturnDateUtc.HasValue)
        {
            throw new InvalidOperationException("Returned issue cannot be updated again.");
        }

        var inventory = await dbContext.Inventories.FirstOrDefaultAsync(x => x.BoardGameId == issue.BoardGameId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for board game {issue.BoardGameId} was not found.");

        var returnDate = request.ReturnDateUtc ?? DateTime.UtcNow;
        issue.ReturnDateUtc = returnDate;
        issue.ConditionGivenIn = request.ConditionGivenIn;
        issue.UpdatedAtUtc = DateTime.UtcNow;
        issue.ModifiedByUser = currentUserService.GetUsername();

        if (returnDate > issue.EndDateUtc)
        {
            var overdueDays = (returnDate.Date - issue.EndDateUtc.Date).Days;
            issue.OverdueCharges = overdueDays * _businessRules.OverdueDailyFeeInr;
            issue.Status = GameIssueStatus.Overdue;
        }
        else
        {
            issue.OverdueCharges = 0;
            issue.Status = GameIssueStatus.Returned;
        }

        var returnedCondition = request.ConditionGivenIn!.Value;
        if (returnedCondition is GameCondition.Lost or GameCondition.Broken)
        {
            inventory.IsMissingOrBroken = true;
        }
        else
        {
            inventory.AvailableInventory += 1;
        }

        inventory.UpdatedAtUtc = DateTime.UtcNow;
        inventory.ModifiedByUser = currentUserService.GetUsername();

        await dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return ToResponse(issue);
    }

    public async Task<PagedResponse<GameIssueResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.GameIssues
            .OrderByDescending(x => x.StartDateUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return new PagedResponse<GameIssueResponse>(items, totalCount, page, pageSize);
    }

    public async Task<GameIssueResponse> GetAsync(int id, CancellationToken cancellationToken)
    {
        var issue = await dbContext.GameIssues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Game issue {id} was not found.");
        return ToResponse(issue);
    }

    private static GameIssueResponse ToResponse(GameIssue issue) =>
        new(
            issue.Id,
            issue.BoardGameId,
            issue.MemberId,
            issue.StartDateUtc,
            issue.EndDateUtc,
            issue.ReturnDateUtc,
            issue.ConditionGivenOut,
            issue.ConditionGivenIn,
            issue.OverdueCharges,
            issue.Status);
}
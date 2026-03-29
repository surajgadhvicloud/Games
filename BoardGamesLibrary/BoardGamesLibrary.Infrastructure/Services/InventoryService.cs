using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Services;

public class InventoryService(BoardGamesDbContext dbContext, ICurrentUserService currentUserService) : IInventoryService
{
    public async Task<InventoryResponse> CreateAsync(CreateInventoryRequest request, CancellationToken cancellationToken)
    {
        var boardGameExists = await dbContext.BoardGames.AnyAsync(x => x.Id == request.BoardGameId, cancellationToken);
        if (!boardGameExists)
        {
            throw new KeyNotFoundException($"Board game {request.BoardGameId} was not found.");
        }

        var existingInventory = await dbContext.Inventories.AnyAsync(x => x.BoardGameId == request.BoardGameId, cancellationToken);
        if (existingInventory)
        {
            throw new InvalidOperationException("Inventory already exists for this board game.");
        }

        var entity = new Inventory
        {
            BoardGameId = request.BoardGameId,
            IsMissingOrBroken = request.IsMissingOrBroken,
            TotalInventory = request.TotalInventory,
            AvailableInventory = request.AvailableInventory,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedByUser = currentUserService.GetUsername()
        };

        dbContext.Inventories.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<InventoryResponse> UpdateAsync(int boardGameId, UpdateInventoryRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Inventories.FirstOrDefaultAsync(x => x.BoardGameId == boardGameId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for board game {boardGameId} was not found.");

        var activeIssues = await dbContext.GameIssues.AnyAsync(
            x => x.BoardGameId == boardGameId && x.ReturnDateUtc == null,
            cancellationToken);
        if (activeIssues && request.TotalInventory < entity.TotalInventory)
        {
            throw new InvalidOperationException("Cannot decrease total inventory while active issues exist.");
        }

        entity.IsMissingOrBroken = request.IsMissingOrBroken;
        entity.TotalInventory = request.TotalInventory;
        entity.AvailableInventory = request.AvailableInventory;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.ModifiedByUser = currentUserService.GetUsername();

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PagedResponse<InventoryResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Inventories
            .OrderBy(x => x.BoardGameId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return new PagedResponse<InventoryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<InventoryResponse> GetByBoardGameIdAsync(int boardGameId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Inventories.FirstOrDefaultAsync(x => x.BoardGameId == boardGameId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for board game {boardGameId} was not found.");
        return ToResponse(entity);
    }

    private static InventoryResponse ToResponse(Inventory entity) =>
        new(entity.Id, entity.BoardGameId, entity.IsMissingOrBroken, entity.TotalInventory, entity.AvailableInventory);
}
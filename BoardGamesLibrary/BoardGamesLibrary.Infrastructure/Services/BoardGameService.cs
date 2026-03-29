using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Services;

public class BoardGameService(BoardGamesDbContext dbContext, ICurrentUserService currentUserService) : IBoardGameService
{
    public async Task<BoardGameResponse> CreateAsync(CreateBoardGameRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.BoardGames.AnyAsync(
            x => x.GameName == request.GameName && x.Version == request.Version,
            cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Board game with the same name and version already exists.");
        }

        var entity = new BoardGame
        {
            GameName = request.GameName.Trim(),
            Version = request.Version.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            Price = request.Price,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedByUser = currentUserService.GetUsername()
        };

        dbContext.BoardGames.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<BoardGameResponse> UpdateAsync(int id, UpdateBoardGameRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.BoardGames.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Board game {id} was not found.");

        entity.GameName = request.GameName.Trim();
        entity.Version = request.Version.Trim();
        entity.ImageUrl = request.ImageUrl?.Trim();
        entity.MinPlayers = request.MinPlayers;
        entity.MaxPlayers = request.MaxPlayers;
        entity.Price = request.Price;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.ModifiedByUser = currentUserService.GetUsername();

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PagedResult<BoardGameResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.BoardGames
            .OrderBy(x => x.GameName)
            .ThenBy(x => x.Version);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<BoardGameResponse>(items, totalCount, page, pageSize);
    }

    public async Task<BoardGameResponse> GetAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.BoardGames.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Board game {id} was not found.");
        return ToResponse(entity);
    }

    private static BoardGameResponse ToResponse(BoardGame entity) =>
        new(entity.Id, entity.GameName, entity.Version, entity.MinPlayers, entity.MaxPlayers, entity.Price, entity.ImageUrl);
}
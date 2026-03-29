using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IBoardGameService
{
    Task<BoardGameResponse> CreateAsync(CreateBoardGameRequest request, CancellationToken cancellationToken);
    Task<BoardGameResponse> UpdateAsync(int id, UpdateBoardGameRequest request, CancellationToken cancellationToken);
    Task<PagedResult<BoardGameResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BoardGameResponse> GetAsync(int id, CancellationToken cancellationToken);
}
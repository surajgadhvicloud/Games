using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IBoardGameService
{
    Task<BoardGameResponse> CreateAsync(CreateBoardGameRequest request, CancellationToken cancellationToken);
    Task<BoardGameResponse> UpdateAsync(int id, UpdateBoardGameRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<BoardGameResponse>> ListAsync(CancellationToken cancellationToken);
    Task<BoardGameResponse> GetAsync(int id, CancellationToken cancellationToken);
}
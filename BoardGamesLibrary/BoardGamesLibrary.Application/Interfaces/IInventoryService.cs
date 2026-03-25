using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryResponse> CreateAsync(CreateInventoryRequest request, CancellationToken cancellationToken);
    Task<InventoryResponse> UpdateAsync(int boardGameId, UpdateInventoryRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryResponse>> ListAsync(CancellationToken cancellationToken);
    Task<InventoryResponse> GetByBoardGameIdAsync(int boardGameId, CancellationToken cancellationToken);
}
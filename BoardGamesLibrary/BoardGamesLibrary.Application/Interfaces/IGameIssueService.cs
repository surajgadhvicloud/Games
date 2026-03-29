using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IGameIssueService
{
    Task<GameIssueResponse> CreateAsync(CreateGameIssueRequest request, CancellationToken cancellationToken);
    Task<GameIssueResponse> UpdateAsync(int id, UpdateGameIssueRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<GameIssueResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<GameIssueResponse> GetAsync(int id, CancellationToken cancellationToken);
}
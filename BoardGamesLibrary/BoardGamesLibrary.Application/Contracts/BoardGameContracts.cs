namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateBoardGameRequest(
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price,
    string? ImageUrl = null);

public sealed record UpdateBoardGameRequest(
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price,
    string? ImageUrl = null);

public sealed record BoardGameResponse(
    int Id,
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price,
    string? ImageUrl);
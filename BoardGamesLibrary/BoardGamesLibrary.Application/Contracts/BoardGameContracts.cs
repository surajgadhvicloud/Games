namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateBoardGameRequest(
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price);

public sealed record UpdateBoardGameRequest(
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price);

public sealed record BoardGameResponse(
    int Id,
    string GameName,
    string Version,
    int MinPlayers,
    int MaxPlayers,
    decimal Price);
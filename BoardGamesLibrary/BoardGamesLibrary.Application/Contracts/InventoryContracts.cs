namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateInventoryRequest(
    int BoardGameId,
    bool IsMissingOrBroken,
    int TotalInventory,
    int AvailableInventory);

public sealed record UpdateInventoryRequest(
    bool IsMissingOrBroken,
    int TotalInventory,
    int AvailableInventory);

public sealed record InventoryResponse(
    int Id,
    int BoardGameId,
    bool IsMissingOrBroken,
    int TotalInventory,
    int AvailableInventory);
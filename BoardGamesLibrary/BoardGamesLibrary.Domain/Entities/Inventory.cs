namespace BoardGamesLibrary.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public int BoardGameId { get; set; }
    public bool IsMissingOrBroken { get; set; }
    public int TotalInventory { get; set; }
    public int AvailableInventory { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? ModifiedByUser { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public BoardGame BoardGame { get; set; } = null!;
} 
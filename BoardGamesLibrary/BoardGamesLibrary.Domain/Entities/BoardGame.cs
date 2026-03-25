using BoardGamesLibrary.Domain.Entities;

namespace BoardGamesLibrary.Domain.Entities;

public class BoardGame
{
    public int Id { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? ModifiedByUser { get; set; }

    public Inventory? Inventory { get; set; }
    public ICollection<GameIssue> Issues { get; set; } = new List<GameIssue>();
}
using BoardGamesLibrary.Domain.Enums;

namespace BoardGamesLibrary.Domain.Entities;

public class GameIssue
{
    public int Id { get; set; }
    public int BoardGameId { get; set; }
    public int MemberId { get; set; }
    public string? PhotoUrlBeforeIssue { get; set; }
    public string? PhotoUrlAfterReturn { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public DateTime? ReturnDateUtc { get; set; }
    public GameCondition ConditionGivenOut { get; set; }
    public GameCondition? ConditionGivenIn { get; set; }
    public decimal OverdueCharges { get; set; }
    public GameIssueStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? ModifiedByUser { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public BoardGame BoardGame { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
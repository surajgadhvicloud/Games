using BoardGamesLibrary.Domain.Enums;

namespace BoardGamesLibrary.Domain.Entities;

public class Member
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserType TypeOfUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? ModifiedByUser { get; set; }

    public ICollection<GameIssue> Issues { get; set; } = new List<GameIssue>();
}

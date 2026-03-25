namespace BoardGamesLibrary.Infrastructure.Configuration;

public class SeederOptions
{
    public const string SectionName = "Seeder";

    public string AdminDefaultPassword { get; set; } = string.Empty;
    public string ManagerDefaultPassword { get; set; } = string.Empty;
    public string DataEntryDefaultPassword { get; set; } = string.Empty;
}
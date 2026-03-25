namespace BoardGamesLibrary.Infrastructure.Configuration;

public class BusinessRulesOptions
{
    public const string SectionName = "BusinessRules";

    public int PremiumMaxActiveIssues { get; set; } = 5;
    public int RegularMaxActiveIssues { get; set; } = 2;
    public int PremiumLoanDays { get; set; } = 30;
    public int RegularLoanDays { get; set; } = 14;
    public decimal OverdueDailyFeeInr { get; set; } = 250m;
}
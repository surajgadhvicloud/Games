using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BoardGamesDbContext dbContext, SeederOptions seederOptions, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var passwordHasher = new PasswordHasher<User>();
        var adminPassword = GetRequiredPassword(seederOptions.AdminDefaultPassword, nameof(seederOptions.AdminDefaultPassword));
        var managerPassword = GetRequiredPassword(seederOptions.ManagerDefaultPassword, nameof(seederOptions.ManagerDefaultPassword));
        var dataEntryPassword = GetRequiredPassword(seederOptions.DataEntryDefaultPassword, nameof(seederOptions.DataEntryDefaultPassword));

        var usersToSeed = new List<User>
        {
            new() { FirstName = "System", LastName = "Admin", Email = "admin@boardgames.local", Username = "admin", Role = UserRole.Admin, CreatedAtUtc = now, ModifiedByUser = "seed" },
            new() { FirstName = "Maya", LastName = "Manager", Email = "manager@boardgames.local", Username = "manager", Role = UserRole.Manager, CreatedAtUtc = now, ModifiedByUser = "seed" },
            new() { FirstName = "Dev", LastName = "Entry", Email = "dataentry@boardgames.local", Username = "dataentry", Role = UserRole.DataEntry, CreatedAtUtc = now, ModifiedByUser = "seed" }
        };

        usersToSeed[0].PasswordHash = passwordHasher.HashPassword(usersToSeed[0], adminPassword);
        usersToSeed[1].PasswordHash = passwordHasher.HashPassword(usersToSeed[1], managerPassword);
        usersToSeed[2].PasswordHash = passwordHasher.HashPassword(usersToSeed[2], dataEntryPassword);

        var existingUsernames = await dbContext.Users
            .Select(x => x.Username)
            .ToListAsync(cancellationToken);
        var usersToInsert = usersToSeed
            .Where(x => !existingUsernames.Contains(x.Username, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (usersToInsert.Count > 0)
        {
            dbContext.Users.AddRange(usersToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var membersToSeed = new List<Member>
        {
            new() { FirstName = "Aarav", MiddleName = "K", LastName = "Sharma", Address = "12 MG Road, Bengaluru", PhoneNumber = "9876543210", Email = "aarav.sharma@example.com", TypeOfUser = UserType.Premium, CreatedAtUtc = now },
            new() { FirstName = "Nisha", MiddleName = null, LastName = "Patel", Address = "44 Residency Road, Hyderabad", PhoneNumber = "9988776655", Email = "nisha.patel@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Rahul", MiddleName = "M", LastName = "Iyer", Address = "21 Anna Salai, Chennai", PhoneNumber = "9123456780", Email = "rahul.iyer@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Saanvi", MiddleName = null, LastName = "Reddy", Address = "6 Banjara Hills, Hyderabad", PhoneNumber = "9000000001", Email = "saanvi.reddy@example.com", TypeOfUser = UserType.Premium, CreatedAtUtc = now },
            new() { FirstName = "Vikram", MiddleName = null, LastName = "Singh", Address = "2 Civil Lines, Delhi", PhoneNumber = "9000000002", Email = "vikram.singh@example.com", TypeOfUser = UserType.Premium, CreatedAtUtc = now },
            new() { FirstName = "Meera", MiddleName = "J", LastName = "Nair", Address = "8 Marine Drive, Kochi", PhoneNumber = "9000000003", Email = "meera.nair@example.com", TypeOfUser = UserType.Premium, CreatedAtUtc = now },
            new() { FirstName = "Arjun", MiddleName = null, LastName = "Kapoor", Address = "17 Park Street, Kolkata", PhoneNumber = "9000000004", Email = "arjun.kapoor@example.com", TypeOfUser = UserType.Premium, CreatedAtUtc = now },
            new() { FirstName = "Karan", MiddleName = null, LastName = "Joshi", Address = "11 FC Road, Pune", PhoneNumber = "9000000005", Email = "karan.joshi@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Anita", MiddleName = null, LastName = "Desai", Address = "5 Ring Road, Ahmedabad", PhoneNumber = "9000000006", Email = "anita.desai@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Rohit", MiddleName = null, LastName = "Das", Address = "4 Main Bazaar, Jaipur", PhoneNumber = "9000000007", Email = "rohit.das@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Isha", MiddleName = null, LastName = "Verma", Address = "27 Sector 18, Noida", PhoneNumber = "9000000008", Email = "isha.verma@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Neel", MiddleName = null, LastName = "Bose", Address = "9 Salt Lake, Kolkata", PhoneNumber = "9000000009", Email = "neel.bose@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Pooja", MiddleName = null, LastName = "Agarwal", Address = "13 Gomti Nagar, Lucknow", PhoneNumber = "9000000010", Email = "pooja.agarwal@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Tarun", MiddleName = null, LastName = "Bhat", Address = "3 MG Circle, Mysuru", PhoneNumber = "9000000011", Email = "tarun.bhat@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now },
            new() { FirstName = "Deepa", MiddleName = null, LastName = "Pillai", Address = "15 East Fort, Trivandrum", PhoneNumber = "9000000012", Email = "deepa.pillai@example.com", TypeOfUser = UserType.Regular, CreatedAtUtc = now }
        };

        var existingMemberEmails = await dbContext.Members
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);
        var membersToInsert = membersToSeed
            .Where(x => !existingMemberEmails.Contains(x.Email, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (membersToInsert.Count > 0)
        {
            dbContext.Members.AddRange(membersToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var boardGamesToSeed = new List<BoardGame>
        {
            new() { GameName = "Monopoly", Version = "Classic", MinPlayers = 2, MaxPlayers = 6, Price = 1499, CreatedAtUtc = now },
            new() { GameName = "Catan", Version = "Base", MinPlayers = 3, MaxPlayers = 4, Price = 3299, CreatedAtUtc = now },
            new() { GameName = "Ticket to Ride", Version = "Europe", MinPlayers = 2, MaxPlayers = 5, Price = 2899, CreatedAtUtc = now },
            new() { GameName = "Pandemic", Version = "Base", MinPlayers = 2, MaxPlayers = 4, Price = 2599, CreatedAtUtc = now },
            new() { GameName = "Carcassonne", Version = "Base", MinPlayers = 2, MaxPlayers = 5, Price = 2299, CreatedAtUtc = now },
            new() { GameName = "Azul", Version = "Base", MinPlayers = 2, MaxPlayers = 4, Price = 2799, CreatedAtUtc = now },
            new() { GameName = "7 Wonders", Version = "Second Edition", MinPlayers = 3, MaxPlayers = 7, Price = 3499, CreatedAtUtc = now },
            new() { GameName = "Splendor", Version = "Base", MinPlayers = 2, MaxPlayers = 4, Price = 1999, CreatedAtUtc = now }
        };

        var existingGameKeys = await dbContext.BoardGames
            .Select(x => new { x.GameName, x.Version })
            .ToListAsync(cancellationToken);
        var gameKeySet = existingGameKeys
            .Select(x => $"{x.GameName}|{x.Version}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var boardGamesToInsert = boardGamesToSeed
            .Where(x => !gameKeySet.Contains($"{x.GameName}|{x.Version}"))
            .ToList();
        if (boardGamesToInsert.Count > 0)
        {
            dbContext.BoardGames.AddRange(boardGamesToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var allGames = await dbContext.BoardGames.AsNoTracking().ToListAsync(cancellationToken);
        var inventoryGameIds = await dbContext.Inventories
            .Select(x => x.BoardGameId)
            .ToListAsync(cancellationToken);
        var inventoryGameIdSet = inventoryGameIds.ToHashSet();

        var inventoriesToInsert = allGames
            .Where(x => !inventoryGameIdSet.Contains(x.Id))
            .Select(x => new Inventory
            {
                BoardGameId = x.Id,
                IsMissingOrBroken = false,
                TotalInventory = 4,
                AvailableInventory = 4,
                CreatedAtUtc = now
            })
            .ToList();
        if (inventoriesToInsert.Count > 0)
        {
            dbContext.Inventories.AddRange(inventoriesToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existingIssueCount = await dbContext.GameIssues.CountAsync(cancellationToken);
        if (existingIssueCount < 5)
        {
            var members = await dbContext.Members.OrderBy(x => x.Id).Take(5).ToListAsync(cancellationToken);
            var games = await dbContext.BoardGames.OrderBy(x => x.Id).Take(5).ToListAsync(cancellationToken);

            if (members.Count >= 5 && games.Count >= 5)
            {
                var referenceDate = DateTime.UtcNow.Date;
                var seededIssues = new List<GameIssue>
                {
                    new()
                    {
                        BoardGameId = games[0].Id,
                        MemberId = members[0].Id,
                        StartDateUtc = referenceDate.AddDays(-17),
                        EndDateUtc = referenceDate.AddDays(-10),
                        ReturnDateUtc = referenceDate.AddDays(-7),
                        ConditionGivenOut = GameCondition.Mint,
                        ConditionGivenIn = GameCondition.CompleteNotMint,
                        OverdueCharges = 750,
                        Status = GameIssueStatus.Overdue,
                        CreatedAtUtc = now
                    },
                    new()
                    {
                        BoardGameId = games[1].Id,
                        MemberId = members[1].Id,
                        StartDateUtc = referenceDate.AddDays(-9),
                        EndDateUtc = referenceDate.AddDays(-2),
                        ReturnDateUtc = referenceDate.AddDays(-4),
                        ConditionGivenOut = GameCondition.Mint,
                        ConditionGivenIn = GameCondition.Mint,
                        OverdueCharges = 0,
                        Status = GameIssueStatus.Returned,
                        CreatedAtUtc = now
                    },
                    new()
                    {
                        BoardGameId = games[2].Id,
                        MemberId = members[2].Id,
                        StartDateUtc = referenceDate.AddDays(-12),
                        EndDateUtc = referenceDate.AddDays(-5),
                        ReturnDateUtc = referenceDate.AddDays(-6),
                        ConditionGivenOut = GameCondition.Mint,
                        ConditionGivenIn = GameCondition.CompleteNotMint,
                        OverdueCharges = 0,
                        Status = GameIssueStatus.Returned,
                        CreatedAtUtc = now
                    },
                    new()
                    {
                        BoardGameId = games[3].Id,
                        MemberId = members[3].Id,
                        StartDateUtc = referenceDate.AddDays(-8),
                        EndDateUtc = referenceDate.AddDays(-1),
                        ReturnDateUtc = referenceDate.AddDays(-2),
                        ConditionGivenOut = GameCondition.Mint,
                        ConditionGivenIn = GameCondition.Mint,
                        OverdueCharges = 0,
                        Status = GameIssueStatus.Returned,
                        CreatedAtUtc = now
                    },
                    new()
                    {
                        BoardGameId = games[4].Id,
                        MemberId = members[4].Id,
                        StartDateUtc = referenceDate.AddDays(-6),
                        EndDateUtc = referenceDate.AddDays(1),
                        ReturnDateUtc = referenceDate,
                        ConditionGivenOut = GameCondition.Mint,
                        ConditionGivenIn = GameCondition.CompleteNotMint,
                        OverdueCharges = 0,
                        Status = GameIssueStatus.Returned,
                        CreatedAtUtc = now
                    }
                };

                dbContext.GameIssues.AddRange(seededIssues);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static string GetRequiredPassword(string value, string optionName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Seeder option '{optionName}' must be configured with a non-empty password.");
        }

        return value;
    }
}
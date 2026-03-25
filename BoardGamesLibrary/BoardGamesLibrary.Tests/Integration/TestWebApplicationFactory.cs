using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Domain.Enums;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BoardGamesLibrary.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "BoardGamesLibrary",
                ["Jwt:Audience"] = "BoardGamesLibraryClient",
                ["Jwt:SecretKey"] = "ChangeThisToAVeryLongSecretKeyForLocalDevOnly123!",
                ["Jwt:ExpiresInMinutes"] = "60"
            };
            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<BoardGamesDbContext>));
            services.AddDbContext<BoardGamesDbContext>(options =>
                options
                    .UseInMemoryDatabase("RoleMatrixTests", DatabaseRoot)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BoardGamesDbContext>();
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
        });
    }

    private static void SeedData(BoardGamesDbContext dbContext)
    {
        var now = DateTime.UtcNow;

        if (!dbContext.Users.Any())
        {
            var hasher = new PasswordHasher<User>();
            var users = new List<User>
            {
                new() { FirstName = "System", LastName = "Admin", Email = "admin@boardgames.local", Username = "admin", Role = UserRole.Admin, CreatedAtUtc = now, ModifiedByUser = "seed" },
                new() { FirstName = "Maya", LastName = "Manager", Email = "manager@boardgames.local", Username = "manager", Role = UserRole.Manager, CreatedAtUtc = now, ModifiedByUser = "seed" },
                new() { FirstName = "Dev", LastName = "Entry", Email = "dataentry@boardgames.local", Username = "dataentry", Role = UserRole.DataEntry, CreatedAtUtc = now, ModifiedByUser = "seed" }
            };

            foreach (var user in users)
            {
                user.PasswordHash = hasher.HashPassword(user, "P@ssw0rd123");
            }

            dbContext.Users.AddRange(users);
        }

        if (!dbContext.Members.Any())
        {
            dbContext.Members.Add(new Member
            {
                FirstName = "Seed",
                LastName = "Member",
                Address = "Seed Address",
                PhoneNumber = "9000000000",
                Email = "seed.member@example.com",
                TypeOfUser = UserType.Regular,
                CreatedAtUtc = now,
                ModifiedByUser = "seed"
            });
        }

        if (!dbContext.BoardGames.Any())
        {
            dbContext.BoardGames.Add(new BoardGame
            {
                GameName = "Seed Game",
                Version = "Base",
                MinPlayers = 2,
                MaxPlayers = 4,
                Price = 1000,
                CreatedAtUtc = now,
                ModifiedByUser = "seed"
            });
            dbContext.SaveChanges();
        }

        var boardGame = dbContext.BoardGames.First();
        if (!dbContext.Inventories.Any())
        {
            dbContext.Inventories.Add(new Inventory
            {
                BoardGameId = boardGame.Id,
                IsMissingOrBroken = false,
                TotalInventory = 3,
                AvailableInventory = 3,
                CreatedAtUtc = now,
                ModifiedByUser = "seed"
            });
        }

        dbContext.SaveChanges();
    }
}
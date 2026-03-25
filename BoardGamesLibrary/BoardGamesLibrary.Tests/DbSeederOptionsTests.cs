using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Configuration;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Tests;

public class DbSeederOptionsTests
{
    [Fact]
    public async Task SeedAsync_UsesConfiguredPasswords()
    {
        var dbContext = CreateDbContext();
        var options = new SeederOptions
        {
            AdminDefaultPassword = "Admin#12345",
            ManagerDefaultPassword = "Manager#12345",
            DataEntryDefaultPassword = "DataEntry#12345"
        };

        await DbSeeder.SeedAsync(dbContext, options, CancellationToken.None);

        var hasher = new PasswordHasher<User>();
        var admin = await dbContext.Users.SingleAsync(x => x.Username == "admin");
        var manager = await dbContext.Users.SingleAsync(x => x.Username == "manager");
        var dataEntry = await dbContext.Users.SingleAsync(x => x.Username == "dataentry");

        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(admin, admin.PasswordHash, options.AdminDefaultPassword));
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(manager, manager.PasswordHash, options.ManagerDefaultPassword));
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(dataEntry, dataEntry.PasswordHash, options.DataEntryDefaultPassword));
    }

    [Fact]
    public async Task SeedAsync_Throws_WhenAnyPasswordMissing()
    {
        var dbContext = CreateDbContext();
        var options = new SeederOptions
        {
            AdminDefaultPassword = "Admin#12345",
            ManagerDefaultPassword = string.Empty,
            DataEntryDefaultPassword = "DataEntry#12345"
        };

        var action = () => DbSeeder.SeedAsync(dbContext, options, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    private static BoardGamesDbContext CreateDbContext()
    {
        var dbOptions = new DbContextOptionsBuilder<BoardGamesDbContext>()
            .UseInMemoryDatabase($"DbSeederOptionsTests-{Guid.NewGuid()}")
            .Options;

        return new BoardGamesDbContext(dbOptions);
    }
}
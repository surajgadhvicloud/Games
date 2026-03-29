using BoardGamesLibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Data;

public class BoardGamesDbContext(DbContextOptions<BoardGamesDbContext> options) : DbContext(options)
{
    public DbSet<BoardGame> BoardGames => Set<BoardGame>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<GameIssue> GameIssues => Set<GameIssue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.ToTable("BoardGames", table =>
            {
                table.HasCheckConstraint("CK_BoardGames_Players", "[MinPlayers] >= 1 AND [MaxPlayers] >= [MinPlayers]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GameName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(2048);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.ModifiedByUser).HasMaxLength(100);
            entity.HasIndex(x => new { x.GameName, x.Version }).IsUnique();
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ModifiedByUser).HasMaxLength(100);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ModifiedByUser).HasMaxLength(100);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", table =>
            {
                table.HasCheckConstraint("CK_RefreshTokens_Expiry", "[ExpiresAtUtc] > [CreatedAtUtc]");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).HasMaxLength(300).IsRequired();
            entity.Property(x => x.RevokeReason).HasMaxLength(200);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventories", table =>
            {
                table.HasCheckConstraint("CK_Inventories_Total", "[TotalInventory] > 0");
                table.HasCheckConstraint("CK_Inventories_Available", "[AvailableInventory] >= 0 AND [AvailableInventory] <= [TotalInventory]");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.BoardGameId).IsUnique();
            entity.Property(x => x.ModifiedByUser).HasMaxLength(100);
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasOne(x => x.BoardGame)
                .WithOne(x => x.Inventory)
                .HasForeignKey<Inventory>(x => x.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameIssue>(entity =>
        {
            entity.ToTable("GameIssues", table =>
            {
                table.HasCheckConstraint("CK_GameIssues_Dates", "[EndDateUtc] >= [StartDateUtc]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.Property(x => x.PhotoUrlBeforeIssue).HasMaxLength(2048);
            entity.Property(x => x.PhotoUrlAfterReturn).HasMaxLength(2048);
            entity.Property(x => x.OverdueCharges).HasPrecision(18, 2);
            entity.Property(x => x.ModifiedByUser).HasMaxLength(100);
            entity.HasIndex(x => x.Status);

            entity.HasOne(x => x.BoardGame)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Member)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
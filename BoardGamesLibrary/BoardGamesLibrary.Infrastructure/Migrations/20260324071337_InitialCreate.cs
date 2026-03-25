using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGamesLibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGames", x => x.Id);
                    table.CheckConstraint("CK_BoardGames_Players", "[MinPlayers] >= 1 AND [MaxPlayers] >= [MinPlayers]");
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TypeOfUser = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardGameId = table.Column<int>(type: "int", nullable: false),
                    IsMissingOrBroken = table.Column<bool>(type: "bit", nullable: false),
                    TotalInventory = table.Column<int>(type: "int", nullable: false),
                    AvailableInventory = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.CheckConstraint("CK_Inventories_Available", "[AvailableInventory] >= 0 AND [AvailableInventory] <= [TotalInventory]");
                    table.CheckConstraint("CK_Inventories_Total", "[TotalInventory] > 0");
                    table.ForeignKey(
                        name: "FK_Inventories_BoardGames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "BoardGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardGameId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConditionGivenOut = table.Column<int>(type: "int", nullable: false),
                    ConditionGivenIn = table.Column<int>(type: "int", nullable: true),
                    OverdueCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameIssues", x => x.Id);
                    table.CheckConstraint("CK_GameIssues_Dates", "[EndDateUtc] >= [StartDateUtc]");
                    table.ForeignKey(
                        name: "FK_GameIssues_BoardGames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "BoardGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameIssues_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardGames_GameName_Version",
                table: "BoardGames",
                columns: new[] { "GameName", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameIssues_BoardGameId",
                table: "GameIssues",
                column: "BoardGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameIssues_MemberId",
                table: "GameIssues",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GameIssues_Status",
                table: "GameIssues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_BoardGameId",
                table: "Inventories",
                column: "BoardGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameIssues");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "BoardGames");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGamesLibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlsForBoardGamesAndGameIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrlAfterReturn",
                table: "GameIssues",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrlBeforeIssue",
                table: "GameIssues",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BoardGames",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrlAfterReturn",
                table: "GameIssues");

            migrationBuilder.DropColumn(
                name: "PhotoUrlBeforeIssue",
                table: "GameIssues");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BoardGames");
        }
    }
}

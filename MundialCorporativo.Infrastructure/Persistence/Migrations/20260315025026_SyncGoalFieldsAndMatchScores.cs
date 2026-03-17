using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundialCorporativo.Infrastructure.Persistence.Migrations
{
    public partial class SyncGoalFieldsAndMatchScores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoalsAgainst",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AwayPoints",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomePoints",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchScores_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchScores_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_MatchId",
                table: "MatchScores",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_PlayerId",
                table: "MatchScores",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchScores");

            migrationBuilder.DropColumn(
                name: "GoalsAgainst",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AwayPoints",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomePoints",
                table: "Matches");
        }
    }
}

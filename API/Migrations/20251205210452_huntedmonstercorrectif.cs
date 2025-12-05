using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Pokemon.Migrations
{
    /// <inheritdoc />
    public partial class huntedmonstercorrectif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HuntedMonsters");

            migrationBuilder.AddColumn<string>(
                name: "PlayerEmail",
                table: "HuntedMonsters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerEmail",
                table: "HuntedMonsters");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "HuntedMonsters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

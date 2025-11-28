using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Pokemon.Migrations
{
    /// <inheritdoc />
    public partial class InstanceMonsterCorrectif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstanceMonstres",
                columns: table => new
                {
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    MonstreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceMonstres", x => new { x.PositionX, x.PositionY });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstanceMonstres");
        }
    }
}

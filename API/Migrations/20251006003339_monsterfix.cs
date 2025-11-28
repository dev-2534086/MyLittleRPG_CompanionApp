using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Pokemon.Migrations
{
    /// <inheritdoc />
    public partial class monsterfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InstanceMonstres_MonstreId",
                table: "InstanceMonstres",
                column: "MonstreId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstanceMonstres_Monster_MonstreId",
                table: "InstanceMonstres",
                column: "MonstreId",
                principalTable: "Monster",
                principalColumn: "idMonster",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstanceMonstres_Monster_MonstreId",
                table: "InstanceMonstres");

            migrationBuilder.DropIndex(
                name: "IX_InstanceMonstres_MonstreId",
                table: "InstanceMonstres");
        }
    }
}

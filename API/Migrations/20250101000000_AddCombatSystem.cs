using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Pokemon.Migrations
{
    /// <inheritdoc />
    public partial class AddCombatSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Niveau",
                table: "InstanceMonstres",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PointsVieActuels",
                table: "InstanceMonstres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsVieMax",
                table: "InstanceMonstres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Attaque",
                table: "InstanceMonstres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Defense",
                table: "InstanceMonstres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "InstanceMonstres",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VilleDomicileX",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "VilleDomicileY",
                table: "Characters",
                type: "int",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Niveau",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "PointsVieActuels",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "PointsVieMax",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "Attaque",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "Defense",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "InstanceMonstres");

            migrationBuilder.DropColumn(
                name: "VilleDomicileX",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "VilleDomicileY",
                table: "Characters");
        }
    }
}

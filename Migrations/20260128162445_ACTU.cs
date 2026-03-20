using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW_hammer.Migrations
{
    /// <inheritdoc />
    public partial class ACTU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BirthDate",
                table: "AnimalCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Coloring",
                table: "AnimalCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "AnimalCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Species",
                table: "AnimalCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AnimalCards");

            migrationBuilder.DropColumn(
                name: "Coloring",
                table: "AnimalCards");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "AnimalCards");

            migrationBuilder.DropColumn(
                name: "Species",
                table: "AnimalCards");
        }
    }
}

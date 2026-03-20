using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW_hammer.Migrations
{
    /// <inheritdoc />
    public partial class AAP3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalPhotos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalCardID = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalPhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AnimalPhotos_AnimalCards_AnimalCardID",
                        column: x => x.AnimalCardID,
                        principalTable: "AnimalCards",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalPhotos_AnimalCardID",
                table: "AnimalPhotos",
                column: "AnimalCardID");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AnimalPhotos");
        }
    }
}

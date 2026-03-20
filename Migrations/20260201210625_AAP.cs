using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW_hammer.Migrations
{
    /// <inheritdoc />
    public partial class AAP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalPhoto_AnimalCards_AnimalCardID",
                table: "AnimalPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalPhoto",
                table: "AnimalPhoto");

            migrationBuilder.RenameTable(
                name: "AnimalPhoto",
                newName: "AnimalPhotos");

            migrationBuilder.RenameIndex(
                name: "IX_AnimalPhoto_AnimalCardID",
                table: "AnimalPhotos",
                newName: "IX_AnimalPhotos_AnimalCardID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalPhotos",
                table: "AnimalPhotos",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPhotos_AnimalCards_AnimalCardID",
                table: "AnimalPhotos",
                column: "AnimalCardID",
                principalTable: "AnimalCards",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalPhotos_AnimalCards_AnimalCardID",
                table: "AnimalPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalPhotos",
                table: "AnimalPhotos");

            migrationBuilder.RenameTable(
                name: "AnimalPhotos",
                newName: "AnimalPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_AnimalPhotos_AnimalCardID",
                table: "AnimalPhoto",
                newName: "IX_AnimalPhoto_AnimalCardID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalPhoto",
                table: "AnimalPhoto",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalPhoto_AnimalCards_AnimalCardID",
                table: "AnimalPhoto",
                column: "AnimalCardID",
                principalTable: "AnimalCards",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

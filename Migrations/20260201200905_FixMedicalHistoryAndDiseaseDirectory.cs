using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CW_hammer.Migrations
{
    /// <inheritdoc />
    public partial class FixMedicalHistoryAndDiseaseDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiseaseDirectories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseDirectories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalCardID = table.Column<int>(type: "int", nullable: false),
                    DiseaseDirectoryID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiseaseState = table.Column<bool>(type: "bit", nullable: false),
                    Complaint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_AnimalCards_AnimalCardID",
                        column: x => x.AnimalCardID,
                        principalTable: "AnimalCards",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_DiseaseDirectories_DiseaseDirectoryID",
                        column: x => x.DiseaseDirectoryID,
                        principalTable: "DiseaseDirectories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_AnimalCardID",
                table: "MedicalHistories",
                column: "AnimalCardID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_DiseaseDirectoryID",
                table: "MedicalHistories",
                column: "DiseaseDirectoryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "DiseaseDirectories");
        }
    }
}

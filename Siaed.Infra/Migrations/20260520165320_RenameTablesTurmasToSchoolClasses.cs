using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siaed.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesTurmasToSchoolClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs referencing Turmas before renaming
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Turmas_ClassId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Turmas_SchoolClassId",
                table: "TurmaTeachers");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Teachers_TeachersId",
                table: "TurmaTeachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaTeachers",
                table: "TurmaTeachers");

            migrationBuilder.DropIndex(
                name: "IX_TurmaTeachers_TeachersId",
                table: "TurmaTeachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Turmas",
                table: "Turmas");

            // Rename both tables (preserves all data)
            migrationBuilder.RenameTable(
                name: "TurmaTeachers",
                newName: "SchoolClassTeachers");

            migrationBuilder.RenameTable(
                name: "Turmas",
                newName: "SchoolClasses");

            // Recreate PKs, indexes and FKs with correct names
            migrationBuilder.AddPrimaryKey(
                name: "PK_SchoolClasses",
                table: "SchoolClasses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SchoolClassTeachers",
                table: "SchoolClassTeachers",
                columns: new[] { "SchoolClassId", "TeachersId" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolClassTeachers_TeachersId",
                table: "SchoolClassTeachers",
                column: "TeachersId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClassTeachers_SchoolClasses_SchoolClassId",
                table: "SchoolClassTeachers",
                column: "SchoolClassId",
                principalTable: "SchoolClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClassTeachers_Teachers_TeachersId",
                table: "SchoolClassTeachers",
                column: "TeachersId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_SchoolClasses_ClassId",
                table: "Students",
                column: "ClassId",
                principalTable: "SchoolClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_SchoolClasses_ClassId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "SchoolClassTeachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SchoolClasses",
                table: "SchoolClasses");

            migrationBuilder.RenameTable(
                name: "SchoolClasses",
                newName: "Turmas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Turmas",
                table: "Turmas",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TurmaTeachers",
                columns: table => new
                {
                    SchoolClassId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TeachersId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaTeachers", x => new { x.SchoolClassId, x.TeachersId });
                    table.ForeignKey(
                        name: "FK_TurmaTeachers_Teachers_TeachersId",
                        column: x => x.TeachersId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TurmaTeachers_Turmas_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TurmaTeachers_TeachersId",
                table: "TurmaTeachers",
                column: "TeachersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Turmas_ClassId",
                table: "Students",
                column: "ClassId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

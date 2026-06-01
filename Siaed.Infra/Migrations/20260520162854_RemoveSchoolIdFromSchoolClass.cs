using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siaed.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSchoolIdFromSchoolClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Teachers_TeachersId",
                table: "TurmaTeachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaTeachers",
                table: "TurmaTeachers");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Turmas_TurmaId",
                table: "TurmaTeachers");

            migrationBuilder.DropIndex(
                name: "IX_TurmaTeachers_TurmaId",
                table: "TurmaTeachers");

            migrationBuilder.DropIndex(
                name: "UX_Students_DocumentId_SchoolId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "TurmaId",
                table: "TurmaTeachers",
                newName: "SchoolClassId");

            migrationBuilder.RenameColumn(
                name: "TurmaId",
                table: "Students",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_TurmaId",
                table: "Students",
                newName: "IX_Students_ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurmaTeachers",
                table: "TurmaTeachers",
                columns: new[] { "SchoolClassId", "TeachersId" });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaTeachers_TeachersId",
                table: "TurmaTeachers",
                column: "TeachersId");

            migrationBuilder.CreateIndex(
                name: "UX_Students_DocumentId",
                table: "Students",
                column: "DocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Turmas_ClassId",
                table: "Students",
                column: "ClassId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaTeachers_Teachers_TeachersId",
                table: "TurmaTeachers",
                column: "TeachersId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaTeachers_Turmas_SchoolClassId",
                table: "TurmaTeachers",
                column: "SchoolClassId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Turmas_ClassId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Teachers_TeachersId",
                table: "TurmaTeachers");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaTeachers_Turmas_SchoolClassId",
                table: "TurmaTeachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurmaTeachers",
                table: "TurmaTeachers");

            migrationBuilder.DropIndex(
                name: "IX_TurmaTeachers_TeachersId",
                table: "TurmaTeachers");

            migrationBuilder.DropIndex(
                name: "UX_Students_DocumentId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "SchoolClassId",
                table: "TurmaTeachers",
                newName: "TurmaId");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "Students",
                newName: "TurmaId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                newName: "IX_Students_TurmaId");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Turmas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Students",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurmaTeachers",
                table: "TurmaTeachers",
                columns: new[] { "TeachersId", "TurmaId" });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaTeachers_TurmaId",
                table: "TurmaTeachers",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "UX_Students_DocumentId_SchoolId",
                table: "Students",
                columns: new[] { "DocumentId", "SchoolId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Turmas_TurmaId",
                table: "Students",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaTeachers_Teachers_TeachersId",
                table: "TurmaTeachers",
                column: "TeachersId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaTeachers_Turmas_TurmaId",
                table: "TurmaTeachers",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

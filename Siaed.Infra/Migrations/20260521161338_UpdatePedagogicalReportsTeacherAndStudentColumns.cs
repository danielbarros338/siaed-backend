using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Siaed.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePedagogicalReportsTeacherAndStudentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "PedagogicalReports");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "PedagogicalReports");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "PedagogicalReports");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "PedagogicalReports",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PedagogicalReports_TeacherId",
                table: "PedagogicalReports",
                newName: "IX_PedagogicalReports_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "PedagogicalReports",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_PedagogicalReports_StudentId",
                table: "PedagogicalReports",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedagogicalReports_Students_StudentId",
                table: "PedagogicalReports",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PedagogicalReports_Users_UserId",
                table: "PedagogicalReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedagogicalReports_Students_StudentId",
                table: "PedagogicalReports");

            migrationBuilder.DropForeignKey(
                name: "FK_PedagogicalReports_Users_UserId",
                table: "PedagogicalReports");

            migrationBuilder.DropIndex(
                name: "IX_PedagogicalReports_StudentId",
                table: "PedagogicalReports");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "PedagogicalReports");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PedagogicalReports",
                newName: "TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_PedagogicalReports_UserId",
                table: "PedagogicalReports",
                newName: "IX_PedagogicalReports_TeacherId");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "PedagogicalReports",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "PedagogicalReports",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "PedagogicalReports",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}

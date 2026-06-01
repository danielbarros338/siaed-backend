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
            DropForeignKeyIfExists(migrationBuilder, "TurmaTeachers", "FK_TurmaTeachers_Teachers_TeachersId");
            DropForeignKeyIfExists(migrationBuilder, "TurmaTeachers", "FK_TurmaTeachers_Turmas_TurmaId");
            DropForeignKeyIfExists(migrationBuilder, "Students", "FK_Students_Turmas_TurmaId");
            DropForeignKeyByColumnIfExists(migrationBuilder, "TurmaTeachers", "TeachersId");
            DropForeignKeyByColumnIfExists(migrationBuilder, "TurmaTeachers", "TurmaId");
            DropForeignKeyByColumnIfExists(migrationBuilder, "Students", "TurmaId");

            DropPrimaryKeyIfExists(migrationBuilder, "TurmaTeachers");
            DropIndexIfExists(migrationBuilder, "TurmaTeachers", "IX_TurmaTeachers_TurmaId");
            DropIndexIfExists(migrationBuilder, "Students", "IX_Students_TurmaId");
            DropIndexIfExists(migrationBuilder, "Students", "UX_Students_DocumentId_SchoolId");

            DropColumnIfExists(migrationBuilder, "Turmas", "SchoolId");
            DropColumnIfExists(migrationBuilder, "Students", "SchoolId");

            RenameColumnIfExists(migrationBuilder, "TurmaTeachers", "TurmaId", "SchoolClassId");
            RenameColumnIfExists(migrationBuilder, "Students", "TurmaId", "ClassId");

            AddPrimaryKeyIfNotExists(migrationBuilder, "TurmaTeachers", "PK_TurmaTeachers", "`SchoolClassId`, `TeachersId`");
            CreateIndexIfNotExists(migrationBuilder, "TurmaTeachers", "IX_TurmaTeachers_TeachersId", "`TeachersId`");
            CreateIndexIfNotExists(migrationBuilder, "Students", "IX_Students_ClassId", "`ClassId`");
            CreateUniqueIndexIfNotExists(migrationBuilder, "Students", "UX_Students_DocumentId", "`DocumentId`");

            AddForeignKeyIfNotExists(migrationBuilder, "Students", "FK_Students_Turmas_ClassId", "ClassId", "Turmas", "Id", "RESTRICT");
            AddForeignKeyIfNotExists(migrationBuilder, "TurmaTeachers", "FK_TurmaTeachers_Teachers_TeachersId", "TeachersId", "Teachers", "Id", "CASCADE");
            AddForeignKeyIfNotExists(migrationBuilder, "TurmaTeachers", "FK_TurmaTeachers_Turmas_SchoolClassId", "SchoolClassId", "Turmas", "Id", "CASCADE");
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

        private static void DropForeignKeyIfExists(MigrationBuilder migrationBuilder, string table, string constraintName)
        {
            migrationBuilder.Sql($@"
SET @constraintName := (
    SELECT CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND CONSTRAINT_NAME = '{constraintName}'
        AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    LIMIT 1
);
SET @sql := IF(@constraintName IS NOT NULL, CONCAT('ALTER TABLE `{table}` DROP FOREIGN KEY `', @constraintName, '`'), 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void DropForeignKeyByColumnIfExists(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($@"
SET @constraintName := (
    SELECT CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND COLUMN_NAME = '{column}'
        AND REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);
SET @sql := IF(@constraintName IS NOT NULL, CONCAT('ALTER TABLE `{table}` DROP FOREIGN KEY `', @constraintName, '`'), 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void DropPrimaryKeyIfExists(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql($@"
SET @hasPrimaryKey := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
);
SET @sql := IF(@hasPrimaryKey > 0, 'ALTER TABLE `{table}` DROP PRIMARY KEY', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void DropIndexIfExists(MigrationBuilder migrationBuilder, string table, string indexName)
        {
            migrationBuilder.Sql($@"
SET @hasIndex := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND INDEX_NAME = '{indexName}'
);
SET @sql := IF(@hasIndex > 0, 'ALTER TABLE `{table}` DROP INDEX `{indexName}`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void DropColumnIfExists(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($@"
SET @hasColumn := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND COLUMN_NAME = '{column}'
);
SET @sql := IF(@hasColumn > 0, 'ALTER TABLE `{table}` DROP COLUMN `{column}`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void RenameColumnIfExists(MigrationBuilder migrationBuilder, string table, string oldName, string newName)
        {
            migrationBuilder.Sql($@"
SET @hasOldColumn := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND COLUMN_NAME = '{oldName}'
);
SET @hasNewColumn := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND COLUMN_NAME = '{newName}'
);
SET @sql := IF(@hasOldColumn > 0 AND @hasNewColumn = 0, 'ALTER TABLE `{table}` RENAME COLUMN `{oldName}` TO `{newName}`', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void AddPrimaryKeyIfNotExists(MigrationBuilder migrationBuilder, string table, string constraintName, string columns)
        {
            migrationBuilder.Sql($@"
SET @hasPrimaryKey := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
);
SET @sql := IF(@hasPrimaryKey = 0, 'ALTER TABLE `{table}` ADD CONSTRAINT `{constraintName}` PRIMARY KEY ({columns})', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void CreateIndexIfNotExists(MigrationBuilder migrationBuilder, string table, string indexName, string columns)
        {
            migrationBuilder.Sql($@"
SET @hasIndex := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND INDEX_NAME = '{indexName}'
);
SET @sql := IF(@hasIndex = 0, 'ALTER TABLE `{table}` ADD INDEX `{indexName}` ({columns})', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void CreateUniqueIndexIfNotExists(MigrationBuilder migrationBuilder, string table, string indexName, string columns)
        {
            migrationBuilder.Sql($@"
SET @hasIndex := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND INDEX_NAME = '{indexName}'
);
SET @sql := IF(@hasIndex = 0, 'ALTER TABLE `{table}` ADD UNIQUE INDEX `{indexName}` ({columns})', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }

        private static void AddForeignKeyIfNotExists(MigrationBuilder migrationBuilder, string table, string constraintName, string column, string principalTable, string principalColumn, string onDelete)
        {
            migrationBuilder.Sql($@"
SET @hasForeignKey := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
        AND TABLE_NAME = '{table}'
        AND CONSTRAINT_NAME = '{constraintName}'
        AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);
SET @sql := IF(@hasForeignKey = 0, 'ALTER TABLE `{table}` ADD CONSTRAINT `{constraintName}` FOREIGN KEY (`{column}`) REFERENCES `{principalTable}` (`{principalColumn}`) ON DELETE {onDelete}', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}

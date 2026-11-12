using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustodialTaskList.Migrations
{
    /// <inheritdoc />
    public partial class InitialTaskDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Shift = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedByInitials = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Shift = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyTasks_TaskTemplateId_ScheduledDate",
                table: "DailyTasks",
                columns: new[] { "TaskTemplateId", "ScheduledDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyTasks");

            migrationBuilder.DropTable(
                name: "TaskTemplates");
        }
    }
}

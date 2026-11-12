using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustodialTaskList.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskRepeatingSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRepeating",
                table: "TaskTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RepeatFrequency",
                table: "TaskTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "TaskTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRepeating",
                table: "TaskTemplates");

            migrationBuilder.DropColumn(
                name: "RepeatFrequency",
                table: "TaskTemplates");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TaskTemplates");
        }
    }
}

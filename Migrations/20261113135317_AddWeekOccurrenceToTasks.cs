using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustodialTaskList.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekOccurrenceToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeekOccurrence",
                table: "TaskTemplates",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeekOccurrence",
                table: "TaskTemplates");
        }
    }
}

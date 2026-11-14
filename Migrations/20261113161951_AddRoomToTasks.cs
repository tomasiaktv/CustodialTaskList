using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustodialTaskList.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Room",
                table: "TaskTemplates",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Room",
                table: "DailyTasks",
                type: "TEXT",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Room",
                table: "TaskTemplates");

            migrationBuilder.DropColumn(
                name: "Room",
                table: "DailyTasks");
        }
    }
}

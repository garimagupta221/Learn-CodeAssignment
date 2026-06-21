using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTimesheetFreezeAndReminderLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TimesheetAccessFrozen",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TimesheetReminderLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reminder1SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reminder2SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFrozen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimesheetReminderLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetReminderLogs_UserId_WeekStart",
                table: "TimesheetReminderLogs",
                columns: new[] { "UserId", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimesheetReminderLogs");

            migrationBuilder.DropColumn(
                name: "TimesheetAccessFrozen",
                table: "Users");
        }
    }
}

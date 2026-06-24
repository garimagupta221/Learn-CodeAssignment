using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrmServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimesheetApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Timesheets_Users_ApprovedBy",
                table: "Timesheets");

            migrationBuilder.DropIndex(
                name: "IX_Timesheets_ApprovedBy",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Timesheets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "Timesheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Timesheets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Timesheets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_ApprovedBy",
                table: "Timesheets",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Timesheets_Users_ApprovedBy",
                table: "Timesheets",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

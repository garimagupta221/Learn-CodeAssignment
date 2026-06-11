using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrmServer.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerUserIdToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerUserId",
                table: "Employees",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_ManagerUserId",
                table: "Employees",
                column: "ManagerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_ManagerUserId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ManagerUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "Employees");
        }
    }
}

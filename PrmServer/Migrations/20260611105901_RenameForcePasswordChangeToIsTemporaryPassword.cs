using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrmServer.Migrations
{
    /// <inheritdoc />
    public partial class RenameForcePasswordChangeToIsTemporaryPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ForcePasswordChange",
                table: "Users",
                newName: "IsTemporaryPassword");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTemporaryPassword",
                table: "Users",
                newName: "ForcePasswordChange");
        }
    }
}

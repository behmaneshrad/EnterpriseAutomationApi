using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseAutomation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Key_IsActive",
                schema: "Auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Key",
                schema: "Auth",
                table: "Permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Auth",
                table: "Permissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Auth",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                schema: "Auth",
                table: "Permissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Key_IsActive",
                schema: "Auth",
                table: "Permissions",
                columns: new[] { "Key", "IsActive" });
        }
    }
}

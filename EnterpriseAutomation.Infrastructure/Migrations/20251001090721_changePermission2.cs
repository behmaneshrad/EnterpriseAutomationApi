using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseAutomation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changePermission2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolesPermissions_Permissions_PermissionId",
                schema: "Auth",
                table: "RolesPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolesPermissions_Roles_RoleId",
                schema: "Auth",
                table: "RolesPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolesPermissions",
                schema: "Auth",
                table: "RolesPermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolesPermissions_PermissionId_RoleId",
                schema: "Auth",
                table: "RolesPermissions");

            migrationBuilder.RenameTable(
                name: "RolesPermissions",
                schema: "Auth",
                newName: "RolePermissions",
                newSchema: "Auth");

            migrationBuilder.RenameIndex(
                name: "IX_RolesPermissions_RoleId",
                schema: "Auth",
                table: "RolePermissions",
                newName: "IX_RolePermissions_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Auth",
                table: "Permissions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                schema: "Auth",
                table: "RolePermissions",
                column: "RolePermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "Auth",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                schema: "Auth",
                table: "RolePermissions",
                column: "PermissionId",
                principalSchema: "Auth",
                principalTable: "Permissions",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                schema: "Auth",
                table: "RolePermissions",
                column: "RoleId",
                principalSchema: "Auth",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                schema: "Auth",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                schema: "Auth",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                schema: "Auth",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "Auth",
                table: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "Auth",
                newName: "RolesPermissions",
                newSchema: "Auth");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_RoleId",
                schema: "Auth",
                table: "RolesPermissions",
                newName: "IX_RolesPermissions_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Auth",
                table: "Permissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolesPermissions",
                schema: "Auth",
                table: "RolesPermissions",
                column: "RolePermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermissions_PermissionId_RoleId",
                schema: "Auth",
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RolesPermissions_Permissions_PermissionId",
                schema: "Auth",
                table: "RolesPermissions",
                column: "PermissionId",
                principalSchema: "Auth",
                principalTable: "Permissions",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolesPermissions_Roles_RoleId",
                schema: "Auth",
                table: "RolesPermissions",
                column: "RoleId",
                principalSchema: "Auth",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

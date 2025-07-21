using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocuFlowAPI.Migrations
{
    public partial class UpdateUserWithRoleEnumAndProfession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profession",
                table: "Users");
        }
    }
}

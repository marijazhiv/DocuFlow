using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocuFlowAPI.Migrations
{
    public partial class AddArchivedAtToDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Documents");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace SparklingHome.Migrations
{
    public partial class removedTestCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "testCol",
                table: "Maid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "testCol",
                table: "Maid",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

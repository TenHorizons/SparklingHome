using Microsoft.EntityFrameworkCore.Migrations;

namespace SparklingHome.Migrations
{
    public partial class pulledagainss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "testCol",
                table: "Maid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "testCol",
                table: "Maid");
        }
    }
}

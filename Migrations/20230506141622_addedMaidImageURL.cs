using Microsoft.EntityFrameworkCore.Migrations;

namespace SparklingHome.Migrations
{
    public partial class addedMaidImageURL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Maid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Maid");
        }
    }
}

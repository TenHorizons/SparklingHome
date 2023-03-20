using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SparklingHome.Migrations
{
    public partial class maid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Maid",
                columns: table => new
                {
                    MaidId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(nullable: true),
                    RegistrationDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: true),
                    ContactNo = table.Column<string>(nullable: true),
                    Salary = table.Column<decimal>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false),
                    WorkingExperienceInYears = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maid", x => x.MaidId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Maid");
        }
    }
}

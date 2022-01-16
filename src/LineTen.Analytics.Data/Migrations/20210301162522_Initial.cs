using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LineTen.Analytics.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    TemperatureC = table.Column<int>(nullable: false),
                    Summary = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForecasts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherForecasts");
        }
    }
}

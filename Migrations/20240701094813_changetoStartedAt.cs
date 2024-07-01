using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_TracNghiem_HTSV.Migrations
{
    /// <inheritdoc />
    public partial class changetoStartedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeTaken",
                table: "TestResults");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "TestResults");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeTaken",
                table: "TestResults",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}

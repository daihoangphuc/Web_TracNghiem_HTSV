using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_TracNghiem_HTSV.Migrations
{
    /// <inheritdoc />
    public partial class addTimeTaken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeTaken",
                table: "TestResults",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeTaken",
                table: "TestResults");
        }
    }
}

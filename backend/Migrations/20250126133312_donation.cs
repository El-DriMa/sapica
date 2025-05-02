using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class donation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Donation");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Donation",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Donation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Donation");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Donation",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Donation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Donation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class AdoptionPostaAndAnimalUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPassport",
                table: "Animal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParasiteFree",
                table: "Animal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sterilized",
                table: "Animal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Vaccinated",
                table: "Animal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "AdoptionPost",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionPost_CityId",
                table: "AdoptionPost",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionPost_City_CityId",
                table: "AdoptionPost",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionPost_City_CityId",
                table: "AdoptionPost");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionPost_CityId",
                table: "AdoptionPost");

            migrationBuilder.DropColumn(
                name: "HasPassport",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "ParasiteFree",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "Sterilized",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "Vaccinated",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AdoptionPost");
        }
    }
}

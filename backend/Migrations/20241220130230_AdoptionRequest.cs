using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sapica_backend.Migrations
{
    /// <inheritdoc />
    public partial class AdoptionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "AdoptionRequest",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "AnimalsBefore",
                table: "AdoptionRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AnyAnimalsBefore",
                table: "AdoptionRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AnyKids",
                table: "AdoptionRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BackyardSize",
                table: "AdoptionRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "AdoptionRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "AdoptionRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyMembers",
                table: "AdoptionRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfKids",
                table: "AdoptionRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AdoptionRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCharacteristic",
                table: "AdoptionRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TimeCommitment",
                table: "AdoptionRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionRequest_CityId",
                table: "AdoptionRequest",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionRequest_City_CityId",
                table: "AdoptionRequest",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionRequest_City_CityId",
                table: "AdoptionRequest");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionRequest_CityId",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "AnimalsBefore",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "AnyAnimalsBefore",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "AnyKids",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "BackyardSize",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "FamilyMembers",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "NumberOfKids",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "PreferredCharacteristic",
                table: "AdoptionRequest");

            migrationBuilder.DropColumn(
                name: "TimeCommitment",
                table: "AdoptionRequest");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "AdoptionRequest",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
